using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PandaKey.Api.Models;
using PandaKey.Api.Repositories;
using PandaKey.Api.Services;

namespace PandaKey.Api.Controllers;

/// <summary>
/// Authentication endpoints. Registration creates a user with a BCrypt-hashed
/// password and the default 'user' role; login verifies the password and
/// returns a signed JWT plus the user profile.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly UsersRepository _users;
    private readonly JwtTokenService _jwt;

    public AuthController(UsersRepository users, JwtTokenService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FullName) ||
            string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest(new { message = "FullName, Email and Password are required" });
        }

        var email = req.Email.Trim();
        if (await _users.EmailExistsAsync(email, ct))
            return Conflict(new { message = "A user with this e-mail already exists" });

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        req.Email = email;
        var newId = await _users.RegisterAsync(req, hash, ct);

        var user = await _users.GetByIdAsync(newId, ct);
        if (user is null)
            return StatusCode(500, new { message = "User created but could not be loaded" });

        var token = _jwt.CreateToken(user);
        return Ok(new AuthResponse { Token = token, Role = user.Role, User = user });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Email and Password are required" });

        var found = await _users.GetByEmailWithHashAsync(req.Email.Trim(), ct);
        if (found is null)
            return Unauthorized(new { message = "Invalid e-mail or password" });

        var (user, passwordHash) = found.Value;

        if (!user.IsActive)
            return Unauthorized(new { message = "Account is deactivated" });

        var ok = !string.IsNullOrEmpty(passwordHash)
                 && BCrypt.Net.BCrypt.Verify(req.Password, passwordHash);
        if (!ok)
            return Unauthorized(new { message = "Invalid e-mail or password" });

        var token = _jwt.CreateToken(user);
        return Ok(new AuthResponse { Token = token, Role = user.Role, User = user });
    }
}
