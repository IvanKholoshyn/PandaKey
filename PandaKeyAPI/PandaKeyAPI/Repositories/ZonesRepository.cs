using Microsoft.Data.SqlClient;
using PandaKey.Api.Data;
using PandaKey.Api.Models;

namespace PandaKey.Api.Repositories;

public sealed class ZonesRepository
{
    private readonly SqlConnectionFactory _factory;
    public ZonesRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<ZoneDto>> GetTopAsync(int top, CancellationToken ct)
    {
        var list = new List<ZoneDto>(top);

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT TOP (@top) ZoneId, Name, Description
FROM Zones
ORDER BY ZoneId;
";
        cmd.Parameters.Add(new SqlParameter("@top", top));

        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new ZoneDto
            {
                ZoneId = rd.GetInt32(0),
                Name = rd.GetString(1),
                Description = rd.IsDBNull(2) ? null : rd.GetString(2)
            });
        }

        return list;
    }

    /// <summary>
    /// Admin listing: every zone with the number of access points and the
    /// number of currently active access rules attached to it.
    /// </summary>
    public async Task<IReadOnlyList<ZoneDto>> GetAllWithCountsAsync(CancellationToken ct)
    {
        var list = new List<ZoneDto>();

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT
  z.ZoneId,
  z.Name,
  z.Description,
  (SELECT COUNT(1) FROM AccessPoints ap WHERE ap.ZoneId = z.ZoneId) AS AccessPointCount,
  (SELECT COUNT(1) FROM AccessRules ar WHERE ar.ZoneId = z.ZoneId AND ar.IsActive = 1) AS ActiveRuleCount
FROM Zones z
ORDER BY z.ZoneId;
";

        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new ZoneDto
            {
                ZoneId = rd.GetInt32(0),
                Name = rd.GetString(1),
                Description = rd.IsDBNull(2) ? null : rd.GetString(2),
                AccessPointCount = rd.GetInt32(3),
                ActiveRuleCount = rd.GetInt32(4)
            });
        }

        return list;
    }

    public async Task<int> CreateAsync(CreateZoneRequest req, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Zones(Name, Description)
OUTPUT INSERTED.ZoneId
VALUES(@name, @desc);
";
        cmd.Parameters.Add(new SqlParameter("@name", req.Name));
        cmd.Parameters.Add(new SqlParameter("@desc", (object?)req.Description ?? DBNull.Value));

        var newIdObj = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(newIdObj);
    }

    public async Task<bool> DeleteAsync(int zoneId, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Zones WHERE ZoneId = @id;";
        cmd.Parameters.Add(new SqlParameter("@id", zoneId));

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }
}
