using Microsoft.AspNetCore.Mvc;
using WebApplication1.StructuralPatterns.Flyweight;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlyweightController : ControllerBase
    {
        // POST /api/flyweight/render
        [HttpPost("render")]
        public IActionResult Render([FromBody] List<FlyweightJobDto> jobs)
            => Ok(FlyweightExample.Run(jobs));

        // GET /api/flyweight/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<FlyweightJobDto>
            {
                // SKU-001 (cung cấp shared data 1 lần)
                new()
                {
                    Sku = "SKU-001", StoreId = "Store-01", Price = 59.9m, Stock = 12, Variant = "Default", GridX = 0, GridY = 0,
                    Product = new ProductDefDto
                    {
                        Name = "Sneaker Alpha", Brand = "BrandX", ImageSize = 1024,
                        Attributes = new Dictionary<string,string> { ["Color"]="Red", ["Size"]="M" }
                    }
                },
                // SKU-002 (cung cấp shared data 1 lần)
                new()
                {
                    Sku = "SKU-002", StoreId = "Store-01", Price = 39.5m, Stock = 7, Variant = "Default", GridX = 0, GridY = 1,
                    Product = new ProductDefDto
                    {
                        Name = "Tee Basic", Brand = "BrandY", ImageSize = 2048,
                        Attributes = new Dictionary<string,string> { ["Color"]="Black", ["Material"]="Cotton" }
                    }
                },
                // Dùng lại SKU-001 và SKU-002 (không cần Product)
                new() { Sku = "SKU-001", StoreId = "Store-01", Price = 58m, Stock = 10, Variant = "Default", GridX = 1, GridY = 0 },
                new() { Sku = "SKU-003", StoreId = "Store-01", Price = 99m, Stock = 3,  Variant = "XL",      GridX = 1, GridY = 1 }, // không có Product -> tự sinh mặc định
                new() { Sku = "SKU-002", StoreId = "Store-01", Price = 39m, Stock = 5,  Variant = "Default", GridX = 2, GridY = 0 },
            };

            return Ok(FlyweightExample.Run(sample));
        }
    }
}
