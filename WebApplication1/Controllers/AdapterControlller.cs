using Design_Patterns.StructuralPatterns.Adapter;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdapterController : ControllerBase
    {
        // POST /api/adapter/pay
        [HttpPost("pay")]
        public IActionResult Pay([FromBody] List<PaymentJobDto> jobs)
        {
            var result = AdapterExample.Run(jobs);
            return Ok(result);
        }

        // GET /api/adapter/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<PaymentJobDto>
            {
                new() { Amount = 50.75m, Currency = "usd", Provider = "oldpay" },
                new() { Amount = 0m,      Currency = "usd", Provider = "oldpay" },
                new() { Amount = 20m,     Currency = "",    Provider = "oldpay" }
            };

            var result = AdapterExample.Run(sample);
            return Ok(result);
        }
    }
}
