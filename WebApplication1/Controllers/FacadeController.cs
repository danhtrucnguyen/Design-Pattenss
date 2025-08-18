using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WebApplication1.StructuralPatterns.Facade;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacadeController : ControllerBase
    {
        [HttpPost("checkout")]
        public IActionResult Checkout([FromBody] List<CheckoutJobDto> jobs)
            => Ok(FacadeExample.Run(jobs));

        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<CheckoutJobDto>
            {
                new()
                {
                    Lines = new()
                    {
                        new OrderLineDto { Sku = "SKU-001", Quantity = 2, UnitPrice = 50 },
                        new OrderLineDto { Sku = "SKU-002", Quantity = 1, UnitPrice = 100 }
                    },
                    ShipTo = new AddressDto
                    {
                        Recipient = "John Doe",
                        Line1 = "123 Main Street",
                        City = "New York",
                        Country = "US",
                        Email = "john@example.com"
                    },
                    Method = "visa",
                    InitialStock = new() { ["SKU-001"] = 10, ["SKU-002"] = 5 },
                    PaymentShouldFail = false
                },
                new()
                {
                    Lines = new() { new OrderLineDto { Sku = "A1", Quantity = 1, UnitPrice = 20 } },
                    ShipTo = new AddressDto
                    {
                        Recipient = "Jane",
                        Line1 = "1 First Ave",
                        City = "Hanoi",
                        Country = "VN",
                        Email = "jane@example.com"
                    },
                    Method = "paypal",
                    InitialStock = new() { ["A1"] = 5 },
                    PaymentShouldFail = true
                }
            };

            return Ok(FacadeExample.Run(sample));
        }
    }
}

//GET http://localhost:5102/api/flyweight/demo
//POST http://localhost:5102/api/flyweight/render

/*
[
  {
    "sku": "SKU-001",
    "storeId": "Store-01",
    "price": 59.9,
    "stock": 12,
    "variant": "Default",
    "gridX": 0,
    "gridY": 0,
    "product": {
      "name": "Sneaker Alpha",
      "brand": "BrandX",
      "imageSize": 1024,
      "attributes": { "Color": "Red", "Size": "M" }
    }
  },
  { "sku": "SKU-002", "storeId": "Store-01", "price": 39.5, "stock": 7,  "variant": "Default", "gridX": 0, "gridY": 1,
    "product": { "name": "Tee Basic", "brand": "BrandY", "imageSize": 2048, "attributes": { "Color": "Black" } }
  },
  { "sku": "SKU-001", "storeId": "Store-01", "price": 58.0, "stock": 10, "variant": "Default", "gridX": 1, "gridY": 0 },
  { "sku": "SKU-003", "storeId": "Store-01", "price": 99.0, "stock": 3,  "variant": "XL",      "gridX": 1, "gridY": 1 },
  { "sku": "SKU-002", "storeId": "Store-01", "price": 39.0, "stock": 5,  "variant": "Default", "gridX": 2, "gridY": 0 }
]
*/