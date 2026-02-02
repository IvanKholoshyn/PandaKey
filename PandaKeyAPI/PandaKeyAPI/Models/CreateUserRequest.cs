namespace PandaKey.Api.Models;

public sealed class CreateUserRequest
{
    public int? DepartmentId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = "$2a$11$lab.fake.hash";
}
