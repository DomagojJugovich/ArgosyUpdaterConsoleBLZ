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

        [HttpGet]
        public async Task<List<Machines.Machine>> Get(CancellationToken cancellationToken)
        {

            //direct fetch from DB
            using MyDbContext myDbContext = new MyDbContext();
            return await myDbContext.ArgosyUpdaterMachines.FromSql($"Select * from ArgosyUpdaterMachines").AsNoTracking().ToListAsync(cancellationToken);
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
