using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PandaKey.Api.Models;
using PandaKey.Api.Repositories;

namespace PandaKey.Api.Controllers;

/// <summary>
/// Administrative endpoints. Every action requires a valid JWT whose role
/// claim equals "admin"; otherwise the framework returns 401/403 and the
/// Angular jwtInterceptor drops the session on 401.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public sealed class AdminController : ControllerBase
{
    private readonly UsersRepository _users;
    private readonly ZonesRepository _zones;

    public AdminController(UsersRepository users, ZonesRepository zones)
    {
        _users = users;
        _zones = zones;
    }

    // ---- Users -------------------------------------------------------------

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers(CancellationToken ct)
        => Ok(await _users.GetAllAsync(ct));

    [HttpPost("users/{id:int}/role")]
    public async Task<ActionResult> SetRole([FromRoute] int id, [FromBody] SetRoleRequest req, CancellationToken ct)
    {
        var role = (req.Role ?? "").Trim().ToLowerInvariant();
        if (role != "admin" && role != "user")
            return BadRequest(new { message = "Role must be 'admin' or 'user'" });

        var updated = await _users.UpdateRoleAsync(id, role, ct);
        return updated ? NoContent() : NotFound(new { message = "User not found" });
    }

    [HttpDelete("users/{id:int}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int id, CancellationToken ct)
    {
        var deleted = await _users.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound(new { message = "User not found" });
    }

    // ---- Zones -------------------------------------------------------------

    [HttpGet("zones")]
    public async Task<ActionResult<IReadOnlyList<ZoneDto>>> GetZones(CancellationToken ct)
        => Ok(await _zones.GetAllWithCountsAsync(ct));

    [HttpPost("zones")]
    public async Task<ActionResult> CreateZone([FromBody] CreateZoneRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Zone name is required" });

        var newId = await _zones.CreateAsync(req, ct);
        return Ok(new { zoneId = newId });
    }

    [HttpDelete("zones/{id:int}")]
    public async Task<ActionResult> DeleteZone([FromRoute] int id, CancellationToken ct)
    {
        var deleted = await _zones.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound(new { message = "Zone not found" });
    }
}
