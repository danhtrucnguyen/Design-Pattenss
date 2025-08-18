using Microsoft.AspNetCore.Mvc;
using WebApplication1.CretionalPatterns.AbstractFactory;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbstractFactoryController : ControllerBase
    {
        // POST /api/abstractfactory/pay  (xử lý batch nhiều giao dịch)
        [HttpPost("pay")]
        public IActionResult Pay([FromBody] List<PaymentJobDto> jobs)
            => Ok(AbstractFactoryExample.Run(jobs));

        // GET /api/abstractfactory/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<PaymentJobDto>
            {
                new() { Provider = "visa",   Amount = 5000,   Currency = "USD" }, // OK
                new() { Provider = "paypal", Amount = 5000,   Currency = "USD" }, // OK
                new() { Provider = "visa",   Amount = 15000,  Currency = "USD" }  // bị chặn (fraud)
            };
            return Ok(AbstractFactoryExample.Run(sample));
        }
    }
}
