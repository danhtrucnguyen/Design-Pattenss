using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.Command;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandController : ControllerBase
    {
        // POST http://localhost:5102/api/command/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<CommandJobDto> jobs)
            => Ok(CommandExample.Run(jobs));

        // GET  http://localhost:5102/api/command/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<CommandJobDto>
            {
                new()
                {
                    Title = "Case - Add → Coupon → Remove → Undo x2",
                    Commands = new()
                    {
                        new CommandDto { Type="add",    Sku="A001", Qty=2, UnitPrice=50 },
                        new CommandDto { Type="add",    Sku="B002", Qty=1, UnitPrice=200 },
                        new CommandDto { Type="coupon", Percent=0.10m, Code="SALE10" },
                        new CommandDto { Type="remove", Sku="A001", Qty=1 },
                        new CommandDto { Type="undo",   Steps=2 } // hoàn tác 2 bước cuối (remove + coupon)
                    }
                }
            };
            return Ok(CommandExample.Run(sample));
        }
    }
}
