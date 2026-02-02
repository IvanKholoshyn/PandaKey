using Microsoft.AspNetCore.Mvc;
using PandaKey.Api.Models;
using PandaKey.Api.Repositories;

namespace PandaKey.Api.Controllers;

[ApiController]
[Route("api/zones")]
public sealed class ZonesController : ControllerBase
{
    private readonly ZonesRepository _repo;
    public ZonesController(ZonesRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ZoneDto>>> Get([FromQuery] int top = 50, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 200);
        return Ok(await _repo.GetTopAsync(top, ct));
    }
}
