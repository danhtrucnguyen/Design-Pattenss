using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.Iterator;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IteratorController : ControllerBase
    {
        // POST http://localhost:5102/api/iterator/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<IteratorJobDto> jobs)
            => Ok(IteratorExample.Run(jobs));

        // GET  http://localhost:5102/api/iterator/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var demo = new List<IteratorJobDto>
            {
                new()
                {
                    Title = "Laptop & Phụ kiện, còn hàng, giá ≤ 1000, rating ≥ 4, pageSize=3",
                    PageSize = 3,
                    Filter = new FilterDto { Categories = new(){"laptop","accessory"}, MaxPrice = 1000, InStockOnly = true, MinRating = 4 },
                    Products = new()
                    {
                        new ProductDto{ Sku="NB-001", Name="Notebook 14", Category="laptop",   Price=899,  Stock=5, Rating=4.5m },
                        new ProductDto{ Sku="NB-002", Name="Notebook 16", Category="laptop",   Price=1299, Stock=3, Rating=4.7m },
                        new ProductDto{ Sku="MS-01",  Name="Mouse",       Category="accessory",Price=25,   Stock=10, Rating=4.2m },
                        new ProductDto{ Sku="KB-01",  Name="Keyboard",    Category="accessory",Price=45,   Stock=0,  Rating=4.0m },
                        new ProductDto{ Sku="HD-1TB", Name="SSD 1TB",     Category="accessory",Price=95,   Stock=7,  Rating=3.9m },
                        new ProductDto{ Sku="MON-27", Name="Monitor 27",  Category="accessory",Price=210,  Stock=2,  Rating=4.6m }
                    }
                }
            };
            return Ok(IteratorExample.Run(demo));
        }
    }
}
