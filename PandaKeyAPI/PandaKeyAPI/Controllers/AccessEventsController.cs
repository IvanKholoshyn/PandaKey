using Microsoft.AspNetCore.Mvc;
using PandaKey.Api.Models;
using PandaKey.Api.Repositories;

namespace PandaKey.Api.Controllers;

[ApiController]
[Route("api/access-events")]
public sealed class AccessEventsController : ControllerBase
{
    private readonly AccessEventsRepository _repo;
    public AccessEventsController(AccessEventsRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccessEventDto>>> GetLatest([FromQuery] int top = 50, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 200);
        return Ok(await _repo.GetLatestAsync(top, ct));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateAccessEventRequest req, CancellationToken ct)
    {
        var result = (req.Result ?? "").Trim().ToUpperInvariant();
        if (result is not ("GRANTED" or "DENIED"))
            return BadRequest(new { message = "Result must be GRANTED or DENIED" });

        if (req.AccessPointId <= 0)
            return BadRequest(new { message = "AccessPointId is required" });

        req.Result = result;

        var newId = await _repo.CreateAsync(req, ct);
        return Created("", new { eventId = newId });
    }
}
