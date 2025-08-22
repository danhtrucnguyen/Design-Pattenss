using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.Visitor;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitorController : ControllerBase
    {
        // POST http://localhost:5102/api/visitor/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<VisitorJobDto> jobs)
            => Ok(VisitorExample.Run(jobs));

        // GET  http://localhost:5102/api/visitor/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<VisitorJobDto>
            {
                new()
                {
                    Title = "Case 1 - Mixed + SALE10",
                    Coupon = "SALE10",
                    Items = new()
                    {
                        new ItemDto { Type="physical", Sku="NB-001", Qty=1, UnitPrice=900,  WeightKg=1.2m },
                        new ItemDto { Type="digital",  Sku="EBOOK-A", Qty=2, UnitPrice=15 },
                        new ItemDto { Type="service",  Sku="SETUP",   Hours=2, HourlyRate=20 }
                    }
                },
                new()
                {
                    Title = "Case 2 - Service + SVC5",
                    Coupon = "SVC5",
                    Items = new()
                    {
                        new ItemDto { Type="service", Sku="CONSULT", Hours=5, HourlyRate=30 }
                    }
                }
            };
            return Ok(VisitorExample.Run(sample));
        }
    }
}
