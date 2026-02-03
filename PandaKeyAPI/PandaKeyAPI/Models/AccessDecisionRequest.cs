namespace PandaKey.Api.Models;

public sealed class AccessDecisionRequest
{
    public int UserId { get; set; }
    public int AccessPointId { get; set; }
    public DateTime? UtcNow { get; set; }
}
