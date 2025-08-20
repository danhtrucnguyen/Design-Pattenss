using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.Mediator;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediatorController : ControllerBase
    {
        // POST http://localhost:5102/api/mediator/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<MediatorJobDto> jobs)
            => Ok(MediatorExample.Run(jobs));

        // GET  http://localhost:5102/api/mediator/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<MediatorJobDto>
            {
                new()
                {
                    Title = "Case - Add → Country(VN) → Coupon(SALE10) → Coupon(FREESHIP) → Remove",
                    Actions = new()
                    {
                        new ActionDto{ Type="add",     Sku="A001", Qty=2, UnitPrice=50,  WeightKg=0.5m },
                        new ActionDto{ Type="add",     Sku="B002", Qty=1, UnitPrice=200, WeightKg=1.2m },
                        new ActionDto{ Type="country", Country="VN" },
                        new ActionDto{ Type="coupon",  Code="SALE10" },
                        new ActionDto{ Type="coupon",  Code="FREESHIP" },
                        new ActionDto{ Type="remove",  Sku="A001", Qty=1 }
                    }
                },
                new()
                {
                    Title = "Case - Country không hỗ trợ & coupon sai",
                    Actions = new()
                    {
                        new ActionDto{ Type="add",     Sku="C003", Qty=1, UnitPrice=40, WeightKg=0.2m },
                        new ActionDto{ Type="country", Country="ZZ" },
                        new ActionDto{ Type="coupon",  Code="UNKNOWN" }
                    }
                }
            };
            return Ok(MediatorExample.Run(sample));
        }
    }
}
