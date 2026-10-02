using ArgosyUpdaterConsoleBLZ.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgosyUpdaterConsoleBLZ.Server.Controllers
{
    [ApiController]
    [Route("[controller]/{action}")]
    public class MachinesController : ControllerBase
    {

        private readonly ILogger<MachinesController> _logger;
        private readonly MyDbContext _db;

        public MachinesController(ILogger<MachinesController> logger, MyDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // Max length of LogChanges/LogErrors returned in the list; full text is served by Logs().
        private const int LogPreviewLength = 200;

        [HttpGet]
        public async Task<List<Machines.Machine>> Get(CancellationToken cancellationToken)
        {

            //direct fetch from DB, logs truncated to a preview (full logs can be several hundred KB per machine)
            return await _db.ArgosyUpdaterMachines.AsNoTracking()
                .Select(m => new Machines.Machine
                {
                    MachineName = m.MachineName,
                    IPadress = m.IPadress,
                    UserName = m.UserName,
                    LastSync = m.LastSync,
                    AppFolderVersions = m.AppFolderVersions,
                    LogChanges = m.LogChanges == null ? null : m.LogChanges.Substring(0, LogPreviewLength),
                    LogErrors = m.LogErrors == null ? null : m.LogErrors.Substring(0, LogPreviewLength),
                    UpdaterTerminalError = m.UpdaterTerminalError,
                    ArgosyUpdaterVersion = m.ArgosyUpdaterVersion
                })
                .ToListAsync(cancellationToken);
        }

        [HttpGet]
        public async Task<ActionResult<Machines.MachineLogs>> Logs(string machineId, CancellationToken cancellationToken)
        {
            var logs = await _db.ArgosyUpdaterMachines.AsNoTracking()
                .Where(m => m.MachineName == machineId)
                .Select(m => new Machines.MachineLogs { LogChanges = m.LogChanges, LogErrors = m.LogErrors })
                .FirstOrDefaultAsync(cancellationToken);

            return logs == null ? NotFound() : logs;
        }

        [HttpGet]
        public async Task<Machines.MachineStats> Stats(CancellationToken cancellationToken)
        {
            var rows = await _db.ArgosyUpdaterMachines.AsNoTracking()
                .Select(m => new { m.ArgosyUpdaterVersion, m.AppFolderVersions, HasError = m.LogErrors != null && m.LogErrors != "" })
                .ToListAsync(cancellationToken);

            var versions = rows.Select(r => Version.TryParse(r.ArgosyUpdaterVersion, out var v) ? v : null).ToList();
            var latest = versions.Where(v => v != null).DefaultIfEmpty().Max();

            var appVersions = rows.Select(r => ParseAppVersions(r.AppFolderVersions)).ToList();
            var latestApps = appVersions.SelectMany(a => a)
                .GroupBy(a => a.App, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Max(a => a.Version)!, StringComparer.OrdinalIgnoreCase);

            return new Machines.MachineStats
            {
                Total = rows.Count,
                WaitingForUpgrade = latest == null ? 0 : versions.Count(v => v == null || v < latest),
                InError = rows.Count(r => r.HasError),
                LatestVersion = latest?.ToString(),
                WaitingForAppUpgrade = appVersions.Count(apps => apps.Any(a => a.Version < latestApps[a.App])),
                LatestAppVersions = latestApps.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key} {kv.Value}").ToList()
            };
        }

        // AppFolderVersions holds one "APPNAME version" entry per line, e.g. "ARGOSY 2026.10.2.2\r\n".
        // Lines without a parseable version are ignored.
        private static List<(string App, Version Version)> ParseAppVersions(string? appFolderVersions)
        {
            var result = new List<(string App, Version Version)>();
            if (string.IsNullOrWhiteSpace(appFolderVersions))
                return result;

            foreach (var line in appFolderVersions.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                int sep = line.LastIndexOf(' ');
                if (sep > 0 && Version.TryParse(line[(sep + 1)..], out var version))
                    result.Add((line[..sep].Trim(), version));
            }
            return result;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string machineId, CancellationToken cancellationToken)
        {

            try
            {
                var mch = await _db.ArgosyUpdaterMachines.FirstOrDefaultAsync(s => s.MachineName == machineId, cancellationToken);
                if (mch == null)
                    return NotFound();

                _db.ArgosyUpdaterMachines.Remove(mch);
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Machine {MachineName} deleted by {User}", machineId, User.Identity?.Name);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete machine {MachineName}", machineId);
                return Problem("Failed to delete machine.");
            }
        }

        // Max threshold accepted from the client (same limit as FetchData.MaxStaleDays).
        private const int MaxStaleDays = 3650;

        // Deletes the machines the user confirmed (body: machine names), but only those whose LastSync is still
        // older than olderThanDays, so machines that synced after the page loaded are kept.
        [HttpDelete]
        public async Task<ActionResult<int>> DeleteStale(int olderThanDays, [FromBody] List<string> machineNames, CancellationToken cancellationToken)
        {
            if (olderThanDays < 1 || olderThanDays > MaxStaleDays)
                return BadRequest($"olderThanDays must be between 1 and {MaxStaleDays}.");
            if (machineNames.Count == 0)
                return BadRequest("machineNames must not be empty.");

            var cutoff = DateTime.Now.AddDays(-olderThanDays);
            try
            {
                var deleted = await _db.ArgosyUpdaterMachines
                    .Where(m => m.MachineName != null && machineNames.Contains(m.MachineName) && m.LastSync < cutoff)
                    .ExecuteDeleteAsync(cancellationToken);

                _logger.LogInformation("{Count} machines not synced since {Cutoff} deleted by {User}", deleted, cutoff, User.Identity?.Name);
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete machines not synced since {Cutoff}", cutoff);
                return Problem("Failed to delete stale machines.");
            }
        }

    }
}
