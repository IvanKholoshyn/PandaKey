using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using PandaKey.Api.Data;

namespace PandaKey.Api.Services;

/// <summary>
/// Exports and restores the whole PandaKey dataset as a single JSON document.
///
/// Export reads every table with a generic SELECT * and serializes the rows.
/// Import runs inside one transaction: it clears the tables (child -> parent)
/// and re-inserts the rows (parent -> child), preserving original primary keys
/// via IDENTITY_INSERT where the table has an identity column. The whole
/// operation is atomic — any failure rolls everything back.
/// </summary>
public sealed class BackupService
{
    private readonly SqlConnectionFactory _factory;

    public BackupService(SqlConnectionFactory factory) => _factory = factory;

    // Parent -> child order (safe for inserts). Deletes use the reverse order.
    private static readonly string[] TableOrder =
    {
        "Departments",
        "ScheduleIntervals",
        "Zones",
        "Users",
        "AccessPoints",
        "AccessRules",
        "AccessEvents"
    };

    public sealed class BackupDocument
    {
        public string ExportedUtc { get; set; } = "";
        public Dictionary<string, List<Dictionary<string, object?>>> Tables { get; set; } = new();
    }

    public sealed class ImportResult
    {
        public int Imported { get; set; }
        public int Tables { get; set; }
    }

    public async Task<byte[]> ExportAsync(CancellationToken ct)
    {
        var doc = new BackupDocument { ExportedUtc = DateTime.UtcNow.ToString("o") };

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        foreach (var table in TableOrder)
        {
            var rows = new List<Dictionary<string, object?>>();

            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM [{table}];";

            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                var row = new Dictionary<string, object?>(rd.FieldCount);
                for (var i = 0; i < rd.FieldCount; i++)
                {
                    var name = rd.GetName(i);
                    row[name] = await rd.IsDBNullAsync(i, ct) ? null : rd.GetValue(i);
                }
                rows.Add(row);
            }
            await rd.CloseAsync();

            doc.Tables[table] = rows;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(doc, options);
        return Encoding.UTF8.GetBytes(json);
    }

    public async Task<ImportResult> ImportAsync(Stream content, CancellationToken ct)
    {
        using var jsonDoc = await JsonDocument.ParseAsync(content, cancellationToken: ct);
        var root = jsonDoc.RootElement;

        if (!root.TryGetProperty("tables", out var tablesEl) &&
            !root.TryGetProperty("Tables", out tablesEl))
        {
            throw new InvalidOperationException("Backup file does not contain a 'tables' section.");
        }

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);

        var result = new ImportResult();

        try
        {
            // 1) Clear existing data, children first.
            foreach (var table in TableOrder.Reverse())
            {
                if (!HasTable(tablesEl, table)) continue;
                var del = conn.CreateCommand();
                del.Transaction = tx;
                del.CommandText = $"DELETE FROM [{table}];";
                await del.ExecuteNonQueryAsync(ct);
            }

            // 2) Insert rows, parents first.
            foreach (var table in TableOrder)
            {
                if (!TryGetTable(tablesEl, table, out var rowsEl)) continue;

                var hasIdentity = await HasIdentityColumnAsync(conn, tx, table, ct);
                if (hasIdentity)
                    await ExecAsync(conn, tx, $"SET IDENTITY_INSERT [{table}] ON;", ct);

                foreach (var rowEl in rowsEl.EnumerateArray())
                {
                    var columns = new List<string>();
                    var paramNames = new List<string>();
                    var values = new List<object>();

                    var idx = 0;
                    foreach (var prop in rowEl.EnumerateObject())
                    {
                        columns.Add($"[{prop.Name}]");
                        var p = $"@p{idx++}";
                        paramNames.Add(p);
                        values.Add(ToClrValue(prop.Value));
                    }

                    if (columns.Count == 0) continue;

                    var insert = conn.CreateCommand();
                    insert.Transaction = tx;
                    insert.CommandText =
                        $"INSERT INTO [{table}] ({string.Join(", ", columns)}) " +
                        $"VALUES ({string.Join(", ", paramNames)});";

                    for (var i = 0; i < values.Count; i++)
                        insert.Parameters.Add(new SqlParameter(paramNames[i], values[i]));

                    await insert.ExecuteNonQueryAsync(ct);
                    result.Imported++;
                }

                if (hasIdentity)
                    await ExecAsync(conn, tx, $"SET IDENTITY_INSERT [{table}] OFF;", ct);

                result.Tables++;
            }

            await tx.CommitAsync(ct);
            return result;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private static bool HasTable(JsonElement tablesEl, string table)
        => tablesEl.TryGetProperty(table, out _);

    private static bool TryGetTable(JsonElement tablesEl, string table, out JsonElement rowsEl)
    {
        if (tablesEl.TryGetProperty(table, out rowsEl) &&
            rowsEl.ValueKind == JsonValueKind.Array)
        {
            return true;
        }
        rowsEl = default;
        return false;
    }

    private static async Task ExecAsync(SqlConnection conn, SqlTransaction tx, string sql, CancellationToken ct)
    {
        var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<bool> HasIdentityColumnAsync(
        SqlConnection conn, SqlTransaction tx, string table, CancellationToken ct)
    {
        var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText =
            "SELECT COUNT(1) FROM sys.columns " +
            "WHERE object_id = OBJECT_ID(@t) AND is_identity = 1;";
        cmd.Parameters.Add(new SqlParameter("@t", table));
        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        return count > 0;
    }

    private static object ToClrValue(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return DBNull.Value;
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Number:
                if (el.TryGetInt64(out var l)) return l;
                if (el.TryGetDecimal(out var dec)) return dec;
                return el.GetDouble();
            case JsonValueKind.String:
                // SQL Server implicitly converts ISO date/time strings to the
                // target column type; plain strings pass through unchanged.
                return el.GetString() ?? (object)DBNull.Value;
            default:
                return el.GetRawText();
        }
    }
}
