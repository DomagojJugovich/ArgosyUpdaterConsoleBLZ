using ArgosyUpdaterConsoleBLZ.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ArgosyUpdaterConsoleBLZ.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Machines> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new Machines
            {
                
            })
            .ToArray();
        }
    }
}