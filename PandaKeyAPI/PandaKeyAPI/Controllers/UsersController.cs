using Microsoft.AspNetCore.Mvc;
using PandaKey.Api.Models;
using PandaKey.Api.Repositories;

namespace PandaKey.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly UsersRepository _repo;
    public UsersController(UsersRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> Get([FromQuery] int top = 50, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 200);
        return Ok(await _repo.GetTopAsync(top, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById([FromRoute] int id, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(id, ct);
        return user is null ? NotFound(new { message = "User not found" }) : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Email))
            return BadRequest(new { message = "FullName and Email are required" });

        var newId = await _repo.CreateAsync(req, ct);
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { userId = newId });
    }
}
