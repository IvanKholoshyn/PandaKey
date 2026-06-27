namespace PandaKey.Api.Models;

/// <summary>Registration payload from the web client.</summary>
public sealed class RegisterRequest
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Password { get; set; } = "";
}

/// <summary>Login payload from the web client.</summary>
public sealed class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

/// <summary>
/// Response returned by both /api/auth/login and /api/auth/register.
/// Shape matches the Angular AuthResponse interface: { token, role, user }.
/// </summary>
public sealed class AuthResponse
{
    public string Token { get; set; } = "";
    public string Role { get; set; } = "user";
    public UserDto User { get; set; } = new();
}

/// <summary>Admin request to change a user's role.</summary>
public sealed class SetRoleRequest
{
    public string Role { get; set; } = "user";
}

/// <summary>Admin request to create a new zone.</summary>
public sealed class CreateZoneRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}
