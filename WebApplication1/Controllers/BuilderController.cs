using Microsoft.AspNetCore.Mvc;
using WebApplication1.CretionalPatterns.Builder;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuilderController : ControllerBase
    {
        // POST /api/builder/build  (nhận nhiều job, mỗi job tạo 1 Order)
        [HttpPost("build")]
        public IActionResult Build([FromBody] List<BuildOrderJobDto> jobs)
            => Ok(BuilderExample.Run(jobs));

        // GET /api/builder/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<BuildOrderJobDto>
            {
                new()
                {
                    CustomerId = "CUST001",
                    Items = new()
                    {
                        new OrderItemDto { Sku = "SKU123", Quantity = 2, UnitPrice = 50 },
                        new OrderItemDto { Sku = "SKU999", Quantity = 1, UnitPrice = 120 }
                    },
                    CouponCode = "SALE10",
                    CouponValue = 10,
                    ShippingMethod = "Express",
                    ShippingAddress = "123 Nga Tu So Street"
                },
                new()
                {
                    CustomerId = "CUST002",
                    Items = new()
                    {
                        new OrderItemDto { Sku = "A1", Quantity = 3, UnitPrice = 20 }
                    },
                    ShippingMethod = "Standard",
                    ShippingAddress = "456 Le Loi, Da Nang"
                }
            };
            return Ok(BuilderExample.Run(sample));
        }
    }
}
