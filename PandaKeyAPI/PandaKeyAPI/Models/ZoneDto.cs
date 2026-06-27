namespace PandaKey.Api.Models;

public sealed class ZoneDto
{
    public int ZoneId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    // Populated only by the admin listing (/api/admin/zones); null elsewhere.
    public int? AccessPointCount { get; set; }
    public int? ActiveRuleCount { get; set; }
}
