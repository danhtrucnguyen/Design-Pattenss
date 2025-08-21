using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.Strategy;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StrategyController : ControllerBase
    {
        // POST http://localhost:5102/api/strategy/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<StrategyJobDto> jobs)
            => Ok(StrategyExample.Run(jobs));

        // GET  http://localhost:5102/api/strategy/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var demo = new List<StrategyJobDto>
            {
                new()
                {
                    Title = "Không giảm",
                    Strategy = new StrategyDto { Type = "none" },
                    Items = new()
                    {
                        new ItemDto { Sku="A001", Qty=2, UnitPrice=50 },
                        new ItemDto { Sku="B002", Qty=1, UnitPrice=200 }
                    }
                },
                new()
                {
                    Title = "Giảm 10%",
                    Strategy = new StrategyDto { Type = "percent", Percent = 0.10m },
                    Items = new()
                    {
                        new ItemDto { Sku="A001", Qty=2, UnitPrice=50 },
                        new ItemDto { Sku="B002", Qty=1, UnitPrice=200 }
                    }
                },
                new()
                {
                    Title = "Giảm cố định 30",
                    Strategy = new StrategyDto { Type = "fixed", Amount = 30m },
                    Items = new()
                    {
                        new ItemDto { Sku="A001", Qty=2, UnitPrice=50 }
                    }
                },
                new()
                {
                    Title = "Bulk: ≥5 sp giảm 15%",
                    Strategy = new StrategyDto { Type = "bulk", Percent = 0.15m, MinQty = 5 },
                    Items = new()
                    {
                        new ItemDto { Sku="C003", Qty=3, UnitPrice=40 },
                        new ItemDto { Sku="D004", Qty=2, UnitPrice=20 }
                    }
                }
            };
            return Ok(StrategyExample.Run(demo));
        }
    }
}
