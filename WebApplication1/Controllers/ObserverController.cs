using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.Observer;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObserverController : ControllerBase
    {
        // POST http://localhost:5102/api/observer/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<ObserverJobDto> jobs)
            => Ok(ObserverExample.Run(jobs));

        // GET  http://localhost:5102/api/observer/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<ObserverJobDto>
            {
                new()
                {
                    Title = "Theo dõi giảm giá & cạn hàng",
                    Product = new ProductDto { Sku="NB-001", Price=1500, Stock=5 },
                    Watchers = new()
                    {
                        new WatcherDto { Type="price", TargetPrice=1200, Name="Alice" },
                        new WatcherDto { Type="stock", Threshold=2,   Name="OpsBot" }
                    },
                    Actions = new()
                    {
                        new ActionDto { Type="price", NewValue=1299 },
                        new ActionDto { Type="stock", NewValue=2 },
                        new ActionDto { Type="price", NewValue=1199 },
                        new ActionDto { Type="stock", NewValue=1 }
                    }
                }
            };
            return Ok(ObserverExample.Run(sample));
        }
    }
}
