
using Microsoft.AspNetCore.Mvc;
using WebApplication1.CretionalPatterns.FactoryMethod;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryMethodController : ControllerBase
    {
        // POST /api/factorymethod/pay  (xử lý batch nhiều giao dịch)
        [HttpPost("pay")]
        public IActionResult Pay([FromBody] List<PaymentJobDto> jobs)
            => Ok(FactoryMethodExample.Run(jobs));

        // GET /api/factorymethod/demo  (ví dụ giống console)
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<PaymentJobDto>
            {
                new() { Provider = "visa",   Amount = 200,     Currency = "USD" },
                new() { Provider = "paypal", Amount = 500_000, Currency = "VND" },
                new() { Provider = "momo",   Amount = -1,      Currency = "USD" } // invalid
            };
            return Ok(FactoryMethodExample.Run(sample));
        }
    }
}
