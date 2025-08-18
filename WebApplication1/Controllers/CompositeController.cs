using Design_Patterns.StructuralPatterns.Composite;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompositeController : ControllerBase
    {
        [HttpPost("calculate")]
        public IActionResult CalculatePrice([FromBody] List<CartBundleDto> bundles)
        {
            var result = CompositeExample.Run(bundles);
            return Ok(result);
        }

        [HttpGet("demo")]
        public IActionResult GetDemo()
        {
            // Dữ liệu mẫu fix cứng
            var sampleData = new List<CartBundleDto>
            {
                new CartBundleDto
                {
                    Name = "Combo 1",
                    DiscountAmount = 20,
                    Items = new List<CartItemDto>
                    {
                        new CartItemDto { Sku = "A001", Quantity = 2, UnitPrice = 50 },
                        new CartItemDto { Sku = "B002", Quantity = 1, UnitPrice = 200 }
                    }
                },
                new CartBundleDto
                {
                    Name = "Combo 2",
                    DiscountAmount = 15,
                    Items = new List<CartItemDto>
                    {
                        new CartItemDto { Sku = "C003", Quantity = 3, UnitPrice = 30 }
                    }
                }
            };

            var result = CompositeExample.Run(sampleData);
            return Ok(result);
        }
    }
}
