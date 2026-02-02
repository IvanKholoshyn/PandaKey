using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PandaKey.Api.Data;

namespace PandaKey.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly SqlConnectionFactory _factory;
    public HealthController(SqlConnectionFactory factory) => _factory = factory;

    [HttpGet("db")]
    public async Task<IActionResult> CheckDb(CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT @@VERSION;";
        var version = (string?)await cmd.ExecuteScalarAsync(ct);

        return Ok(new
        {
            ok = true,
            utc = DateTime.UtcNow,
            sqlServer = version
        });
    }
}
