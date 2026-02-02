namespace PandaKey.Api.Models;

public sealed class CreateAccessEventRequest
{
    public DateTime? EventTime { get; set; } // optional
    public int? UserId { get; set; }
    public int AccessPointId { get; set; }
    public int? CredentialId { get; set; }

    // "GRANTED" / "DENIED"
    public string Result { get; set; } = "GRANTED";
    public string? Reason { get; set; }
}
