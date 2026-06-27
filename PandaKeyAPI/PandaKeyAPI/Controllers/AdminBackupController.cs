using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PandaKey.Api.Services;

namespace PandaKey.Api.Controllers;

/// <summary>
/// Admin-only backup endpoints. Export streams a JSON dump of the whole
/// dataset; import restores a previously exported file in a single transaction.
/// Both require a JWT with role=admin.
/// </summary>
[ApiController]
[Route("api/admin/backup")]
[Authorize(Roles = "admin")]
public sealed class AdminBackupController : ControllerBase
{
    private readonly BackupService _backup;

    public AdminBackupController(BackupService backup) => _backup = backup;

    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var bytes = await _backup.ExportAsync(ct);
        var fileName = $"pandakey-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        return File(bytes, "application/json", fileName);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _backup.ImportAsync(stream, ct);
            return Ok(new { imported = result.Imported, tables = result.Tables });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Import failed: {ex.Message}" });
        }
    }
}
