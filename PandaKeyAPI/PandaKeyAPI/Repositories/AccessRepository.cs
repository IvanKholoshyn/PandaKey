using Microsoft.Data.SqlClient;
using PandaKey.Api.Data;
using PandaKey.Api.Models;

namespace PandaKey.Api.Repositories;

public sealed class AccessEventsRepository
{
    private readonly SqlConnectionFactory _factory;
    public AccessEventsRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<AccessEventDto>> GetLatestAsync(int top, CancellationToken ct)
    {
        var list = new List<AccessEventDto>(top);

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT TOP (@top)
  EventId, EventTime, UserId, AccessPointId, CredentialId, Result, Reason
FROM AccessEvents
ORDER BY EventTime DESC;
";
        cmd.Parameters.Add(new SqlParameter("@top", top));

        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new AccessEventDto
            {
                EventId = rd.GetInt64(0),
                EventTime = rd.GetDateTime(1),
                UserId = rd.IsDBNull(2) ? null : rd.GetInt32(2),
                AccessPointId = rd.GetInt32(3),
                CredentialId = rd.IsDBNull(4) ? null : rd.GetInt32(4),
                Result = rd.GetString(5),
                Reason = rd.IsDBNull(6) ? null : rd.GetString(6),
            });
        }

        return list;
    }

    public async Task<long> CreateAsync(CreateAccessEventRequest req, CancellationToken ct)
    {
        var eventTime = req.EventTime ?? DateTime.UtcNow;

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO AccessEvents(EventTime, UserId, AccessPointId, CredentialId, Result, Reason)
OUTPUT INSERTED.EventId
VALUES(@eventTime, @userId, @apId, @credId, @result, @reason);
";

        cmd.Parameters.Add(new SqlParameter("@eventTime", eventTime));
        cmd.Parameters.Add(new SqlParameter("@userId", (object?)req.UserId ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@apId", req.AccessPointId));
        cmd.Parameters.Add(new SqlParameter("@credId", (object?)req.CredentialId ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@result", req.Result));
        cmd.Parameters.Add(new SqlParameter("@reason", (object?)req.Reason ?? DBNull.Value));

        var newIdObj = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(newIdObj);
    }
}
