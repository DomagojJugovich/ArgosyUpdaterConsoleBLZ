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

        public MachinesController(ILogger<MachinesController> logger)
        {
            _logger = logger;
        }

        // Max length of LogChanges/LogErrors returned in the list; full text is served by Logs().
        private const int LogPreviewLength = 200;

        [HttpGet]
        public async Task<List<Machines.Machine>> Get(CancellationToken cancellationToken)
        {

            //direct fetch from DB, logs truncated to a preview (full logs can be several hundred KB per machine)
            using MyDbContext myDbContext = new MyDbContext();
            return await myDbContext.ArgosyUpdaterMachines.AsNoTracking()
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
            using MyDbContext myDbContext = new MyDbContext();
            var logs = await myDbContext.ArgosyUpdaterMachines.AsNoTracking()
                .Where(m => m.MachineName == machineId)
                .Select(m => new Machines.MachineLogs { LogChanges = m.LogChanges, LogErrors = m.LogErrors })
                .FirstOrDefaultAsync(cancellationToken);

            return logs == null ? NotFound() : logs;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string machineId, CancellationToken cancellationToken)
        {

            try
            {
                using MyDbContext myDbContext = new MyDbContext();
                var mch = await myDbContext.ArgosyUpdaterMachines.FirstOrDefaultAsync(s => s.MachineName == machineId, cancellationToken);
                if (mch == null)
                    return NotFound();

                myDbContext.ArgosyUpdaterMachines.Remove(mch);
                await myDbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Machine {MachineName} deleted by {User}", machineId, User.Identity?.Name);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete machine {MachineName}", machineId);
                return Problem("Failed to delete machine.");
            }
        }

    }
}
