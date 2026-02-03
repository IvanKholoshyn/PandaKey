using PandaKey.Api.Models;
using PandaKey.Api.Repositories;

namespace PandaKey.Api.Services;

public sealed class AccessDecisionService
{
    private readonly AccessDecisionRepository _repo;

    public AccessDecisionService(AccessDecisionRepository repo) => _repo = repo;

    public async Task<AccessDecisionResponse> DecideAsync(int userId, int accessPointId, DateTime utcNow, CancellationToken ct)
    {
        var ctx = await _repo.GetContextAsync(userId, accessPointId, ct);
        if (ctx is null)
            return Deny(utcNow, "User or AccessPoint not found");

        var (userActive, apActive, zoneId) = ctx.Value;

        if (!userActive) return Deny(utcNow, "User is inactive");
        if (!apActive) return Deny(utcNow, "AccessPoint is inactive");

        var scheduleIds = await _repo.GetActiveRuleScheduleIdsAsync(userId, zoneId, utcNow, ct);
        if (scheduleIds.Count == 0)
            return Deny(utcNow, "No active access rule for this zone");

        var dow = ((int)utcNow.DayOfWeek == 0) ? 7 : (int)utcNow.DayOfWeek;
        var time = utcNow.TimeOfDay;

        var ok = await _repo.IsWithinAnyScheduleIntervalAsync(scheduleIds, dow, time, ct);
        if (!ok) return Deny(utcNow, "Outside allowed schedule");

        return new AccessDecisionResponse { Granted = true, Reason = "Allowed by rule and schedule", Utc = utcNow };
    }

    private static AccessDecisionResponse Deny(DateTime utc, string reason)
        => new() { Granted = false, Reason = reason, Utc = utc };
}
