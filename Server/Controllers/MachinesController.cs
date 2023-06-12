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
        public List<Machines.Machine> Get()
        {

            //direct fetch from DB
            MyDbContext myDbContext = new MyDbContext();
            return myDbContext.ArgosyUpdaterMachines.FromSql($"Select * from ArgosyUpdaterMachines").ToList();
        }

        [HttpGet]
        public string Delete(string machineId)
        {

            try
            {
                MyDbContext myDbContext = new MyDbContext();
                var mch = myDbContext.ArgosyUpdaterMachines.Where(s => s.MachineName == machineId).FirstOrDefault();
                myDbContext.ArgosyUpdaterMachines.Remove(mch);

                myDbContext.SaveChanges();

                return "SUCCESS";
            }
            catch (Exception ex) { 
                return "FAIL : " + ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

    }
}