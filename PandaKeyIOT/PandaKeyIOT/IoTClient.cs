using System.Net.Http.Json;

public sealed class IoTClient
{
    private readonly HttpClient _http;
    private readonly IoTSettings _settings;
    private readonly EventQueue _queue;

    public IoTClient(HttpClient http, IoTSettings settings, EventQueue queue)
    {
        _http = http;
        _settings = settings;
        _queue = queue;
    }

    public async Task<bool> TryFlushQueueAsync(CancellationToken ct)
    {
        var items = await _queue.ReadAllAsync(ct);
        if (items.Count == 0) return true;

        Console.WriteLine($"[QUEUE] pending: {items.Count}. Trying to flush...");

        foreach (var item in items)
        {
            var ok = await TryPostEventAsync(new CreateAccessEventRequest
            {
                EventTime = item.UtcTime,
                UserId = item.UserId,
                AccessPointId = item.AccessPointId,
                Result = item.Result,
                Reason = item.Reason
            }, ct);

            if (!ok)
            {
                Console.WriteLine("[QUEUE] flush failed, will retry later.");
                return false;
            }
        }

        _queue.Clear();
        Console.WriteLine("[QUEUE] flushed OK.");
        return true;
    }

    public async Task<(bool ok, AccessDecisionResponse? decision, string? error)> TryDecideAsync(
        int userId, int accessPointId, CancellationToken ct)
    {
        var req = new AccessDecisionRequest
        {
            UserId = userId,
            AccessPointId = accessPointId,
            UtcNow = DateTime.UtcNow
        };

        for (int attempt = 0; attempt <= _settings.RetryCount; attempt++)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("/api/access/decide", req, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    var text = await resp.Content.ReadAsStringAsync(ct);
                    return (false, null, $"HTTP {(int)resp.StatusCode}: {text}");
                }

                var body = await resp.Content.ReadFromJsonAsync<AccessDecisionResponse>(cancellationToken: ct);
                return (true, body, null);
            }
            catch (Exception ex) when (attempt < _settings.RetryCount)
            {
                await Task.Delay(300, ct);
                if (attempt == _settings.RetryCount - 1) break;
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        return (false, null, "network/timeout");
    }

    public async Task<bool> TryPostEventAsync(CreateAccessEventRequest e, CancellationToken ct)
    {
        for (int attempt = 0; attempt <= _settings.RetryCount; attempt++)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("/api/access-events", e, ct);
                if (resp.IsSuccessStatusCode) return true;

                return false;
            }
            catch when (attempt < _settings.RetryCount)
            {
                await Task.Delay(300, ct);
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public async Task HandleScanAsync(int userId, CancellationToken ct)
    {
        var apId = _settings.AccessPointId;


        await TryFlushQueueAsync(ct);


        var (ok, decision, error) = await TryDecideAsync(userId, apId, ct);

        if (!ok || decision is null)
        {
            Console.WriteLine($"[OFFLINE] cannot decide via server: {error}");


            var qe = new QueuedEvent
            {
                UtcTime = DateTime.UtcNow,
                AccessPointId = apId,
                UserId = userId,
                Result = "DENIED",
                Reason = "Offline: cannot reach server"
            };
            await _queue.EnqueueAsync(qe, ct);

            Console.WriteLine("[ACTUATOR] door stays LOCKED");
            return;
        }


        if (decision.Granted)
            Console.WriteLine("[ACTUATOR] door UNLOCK (simulate)");
        else
            Console.WriteLine("[ACTUATOR] door stays LOCKED");

        var ev = new CreateAccessEventRequest
        {
            EventTime = DateTime.UtcNow,
            UserId = userId,
            AccessPointId = apId,
            Result = decision.Granted ? "GRANTED" : "DENIED",
            Reason = decision.Granted ? null : decision.Reason
        };

        var posted = await TryPostEventAsync(ev, ct);
        if (!posted)
        {
            Console.WriteLine("[QUEUE] cannot post event, saving locally...");
            await _queue.EnqueueAsync(new QueuedEvent
            {
                UtcTime = ev.EventTime ?? DateTime.UtcNow,
                AccessPointId = ev.AccessPointId,
                UserId = ev.UserId,
                Result = ev.Result,
                Reason = ev.Reason
            }, ct);
        }
    }
}
