using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WebApplication1.CretionalPatterns.Prototype;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrototypeController : ControllerBase
    {
        // POST /api/prototype/clone
        [HttpPost("clone")]
        public IActionResult Clone([FromBody] List<PrototypeJobDto> jobs)
            => Ok(PrototypeExample.Run(jobs));

        // GET /api/prototype/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<PrototypeJobDto>
            {
                new()
                {
                    Base = new ProductDto
                    {
                        Sku = "TSHIRT001",
                        Name = "T-Shirt",
                        BasePrice = 20,
                        Category = "Clothing",
                        Tags = new() { "cotton", "unisex" },
                        Attributes = new() { ["size"]="M", ["color"]="white" },
                        Media = new MediaDto { Url = "tshirt.jpg", ImageSize = 3 } // bytes giả lập
                    },
                    Variants = new()
                    {
                        new VariantRequestDto { SkuSuffix = "RED-L",  DeltaPrice = 5  },
                        new VariantRequestDto { SkuSuffix = "BLUE-S", DeltaPrice = 0  }
                    }
                }
            };
            return Ok(PrototypeExample.Run(sample));
        }
    }
}
