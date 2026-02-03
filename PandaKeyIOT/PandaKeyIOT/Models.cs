using System.Text.Json.Serialization;

public sealed class IoTSettings
{
    public string ServerBaseUrl { get; set; } = "https://localhost:7209";
    public int AccessPointId { get; set; } = 1;
    public int TimeoutSeconds { get; set; } = 5;
    public int RetryCount { get; set; } = 2;
    public string QueueFile { get; set; } = "queue.jsonl";
}

public sealed class AccessDecisionRequest
{
    public int UserId { get; set; }
    public int AccessPointId { get; set; }
    public DateTime? UtcNow { get; set; }
}

public sealed class AccessDecisionResponse
{
    public bool Granted { get; set; }
    public string Result { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateTime Utc { get; set; }
}

public sealed class CreateAccessEventRequest
{
    public DateTime? EventTime { get; set; }
    public int? UserId { get; set; }
    public int AccessPointId { get; set; }
    public int? CredentialId { get; set; }
    public string Result { get; set; } = "GRANTED";
    public string? Reason { get; set; }
}

public sealed class QueuedEvent
{
    public DateTime UtcTime { get; set; }
    public int AccessPointId { get; set; }
    public int? UserId { get; set; }
    public string Result { get; set; } = "DENIED";
    public string? Reason { get; set; }
}
