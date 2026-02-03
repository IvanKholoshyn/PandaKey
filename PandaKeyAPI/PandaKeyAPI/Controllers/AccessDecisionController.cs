using Microsoft.AspNetCore.Mvc;
using PandaKey.Api.Models;
using PandaKey.Api.Repositories;
using PandaKey.Api.Services;

namespace PandaKey.Api.Controllers;

[ApiController]
[Route("api/access")]
public sealed class AccessDecisionController : ControllerBase
{
    private readonly AccessDecisionService _service;
    private readonly AccessEventsRepository _events;

    public AccessDecisionController(AccessDecisionService service, AccessEventsRepository events)
    {
        _service = service;
        _events = events;
    }

    [HttpPost("decide")]
    public async Task<ActionResult<AccessDecisionResponse>> Decide([FromBody] AccessDecisionRequest req, CancellationToken ct)
    {
        if (req.UserId <= 0 || req.AccessPointId <= 0)
            return BadRequest(new { message = "UserId and AccessPointId are required" });

        var utcNow = req.UtcNow ?? DateTime.UtcNow;
        var decision = await _service.DecideAsync(req.UserId, req.AccessPointId, utcNow, ct);

        await _events.CreateAsync(new CreateAccessEventRequest
        {
            EventTime = utcNow,
            UserId = req.UserId,
            AccessPointId = req.AccessPointId,
            CredentialId = null,
            Result = decision.Result,
            Reason = decision.Granted ? null : decision.Reason
        }, ct);

        return Ok(decision);
    }
}
