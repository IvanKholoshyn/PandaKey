namespace PandaKey.Api.Models;

public sealed class AccessEventDto
{
    public long EventId { get; set; }
    public DateTime EventTime { get; set; }
    public int? UserId { get; set; }
    public int AccessPointId { get; set; }
    public int? CredentialId { get; set; }
    public string Result { get; set; } = "";
    public string? Reason { get; set; }
}
