
using Microsoft.AspNetCore.Mvc;
using WebApplication1.CretionalPatterns.Singleton;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SingletonController : ControllerBase
    {
        // POST /api/singleton/run  (ghi log theo batch)
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<LogJobDto> jobs)
            => Ok(SingletonExample.Run(jobs));

        // GET /api/singleton/demo  (ví dụ giống console)
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<LogJobDto>
            {
                new()
                {
                    Messages = new()
                    {
                        "Thong bao tu Logger 1",
                        "Thong bao tu Logger 2"
                    }
                }
            };
            return Ok(SingletonExample.Run(sample));
        }
    }
}
