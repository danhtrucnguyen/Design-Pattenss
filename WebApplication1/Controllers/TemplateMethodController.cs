using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.TemplateMethod;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplateController : ControllerBase
    {
        // POST http://localhost:5102/api/template/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<BeverageJobDto> jobs)
            => Ok(TemplateExample.Run(jobs));

        // GET  http://localhost:5102/api/template/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<BeverageJobDto>
            {
                new() { Title = "Cà phê với sữa & đường", Type = "coffee", WithCondiments = true },
                new() { Title = "Trà không chanh",        Type = "tea",    WithCondiments = false, WaterTempC = 95 },
                new() { Title = "Cà phê đen",             Type = "coffee", WithCondiments = false }
            };
            return Ok(TemplateExample.Run(sample));
        }
    }
}
