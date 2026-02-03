namespace PandaKey.Api.Models;

public sealed class AccessDecisionResponse
{
    public bool Granted { get; set; }
    public string Result => Granted ? "GRANTED" : "DENIED";
    public string Reason { get; set; } = "";
    public DateTime Utc { get; set; }
}
