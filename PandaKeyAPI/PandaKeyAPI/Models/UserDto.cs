namespace PandaKey.Api.Models;

public sealed class UserDto
{
    public int UserId { get; set; }
    public int? DepartmentId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
