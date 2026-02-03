using Microsoft.Data.SqlClient;
using PandaKey.Api.Data;

namespace PandaKey.Api.Repositories;

public sealed class AccessDecisionRepository
{
    private readonly SqlConnectionFactory _factory;
    public AccessDecisionRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<(bool userActive, bool accessPointActive, int zoneId)?> GetContextAsync(
        int userId, int accessPointId, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT
  u.IsActive AS UserActive,
  ap.IsActive AS AccessPointActive,
  ap.ZoneId
FROM Users u
JOIN AccessPoints ap ON ap.AccessPointId = @apId
WHERE u.UserId = @userId;
";
        cmd.Parameters.Add(new SqlParameter("@userId", userId));
        cmd.Parameters.Add(new SqlParameter("@apId", accessPointId));

        await using var rd = await cmd.ExecuteReaderAsync(ct);
        if (!await rd.ReadAsync(ct)) return null;

        return (rd.GetBoolean(0), rd.GetBoolean(1), rd.GetInt32(2));
    }

    public async Task<IReadOnlyList<int>> GetActiveRuleScheduleIdsAsync(
        int userId, int zoneId, DateTime utcNow, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT ar.ScheduleId
FROM AccessRules ar
WHERE ar.UserId = @userId
  AND ar.ZoneId = @zoneId
  AND ar.IsActive = 1
  AND (@d >= ISNULL(ar.ValidFrom, '1900-01-01'))
  AND (@d <= ISNULL(ar.ValidTo,   '9999-12-31'));
";
        cmd.Parameters.Add(new SqlParameter("@userId", userId));
        cmd.Parameters.Add(new SqlParameter("@zoneId", zoneId));
        cmd.Parameters.Add(new SqlParameter("@d", utcNow.Date));

        var result = new List<int>();
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
            result.Add(rd.GetInt32(0));

        return result;
    }

    public async Task<bool> IsWithinAnyScheduleIntervalAsync(
        IEnumerable<int> scheduleIds, int dayOfWeek1to7, TimeSpan time, CancellationToken ct)
    {
        var ids = scheduleIds.Distinct().ToArray();
        if (ids.Length == 0) return false;

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var parameters = ids.Select((id, idx) => new SqlParameter($"@s{idx}", id)).ToArray();
        var inClause = string.Join(", ", parameters.Select(p => p.ParameterName));

        var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
SELECT TOP(1) 1
FROM ScheduleIntervals si
WHERE si.ScheduleId IN ({inClause})
  AND si.DayOfWeek = @dow
  AND @t >= si.StartTime
  AND @t <= si.EndTime;
";
        foreach (var p in parameters) cmd.Parameters.Add(p);
        cmd.Parameters.Add(new SqlParameter("@dow", dayOfWeek1to7));
        cmd.Parameters.Add(new SqlParameter("@t", time));

        var obj = await cmd.ExecuteScalarAsync(ct);
        return obj is not null;
    }
}
