using Design_Patterns.StructuralPatterns.Decorator;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.StructuralPatterns.Decorator;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DecoratorController : ControllerBase
    {
        // POST /api/decorator/price
        [HttpPost("price")]
        public IActionResult Calculate([FromBody] List<PriceJobDto> jobs)
            => Ok(DecoratorExample.Run(jobs));

        // GET /api/decorator/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<PriceJobDto>
            {
                new()
                {
                    Country = "US",
                    ShippingMethod = "Express",
                    Items = new()
                    {
                        new OrderItemDto { Sku = "SKU-001", Quantity = 2, UnitPrice = 50 },
                        new OrderItemDto { Sku = "SKU-002", Quantity = 1, UnitPrice = 100 }
                    },
                    TaxRate = 0.08m,
                    CouponPercent = 0.10m,
                    Pipeline = new() { "shipping", "tax", "coupon" }
                },
                new()
                {
                    Country = "US",
                    ShippingMethod = "Standard",
                    Items = new()
                    {
                        new OrderItemDto { Sku = "A1", Quantity = 1, UnitPrice = 30 },
                        new OrderItemDto { Sku = "B2", Quantity = 3, UnitPrice = 20 }
                    },
                    TaxRate = 0.00m,
                    CouponPercent = 0.20m,
                    Pipeline = new() { "shipping", "coupon" } // không tính thuế
                }
            };

            return Ok(DecoratorExample.Run(sample));
        }
    }
}
