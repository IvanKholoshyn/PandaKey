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
}
