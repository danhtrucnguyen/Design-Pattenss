using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WebApplication1.BehavioralPatterns.ChainEcom;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChainEcomController : ControllerBase
    {
        // POST http://localhost:5102/api/chainecom/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<EcomJobDto> jobs)
            => Ok(ChainEcomExample.Run(jobs));

        // GET  http://localhost:5102/api/chainecom/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var demo = new List<EcomJobDto>
            {
                new()
                {
                    Title = "Case 1 - Hợp lệ",
                    Orders = new()
                    {
                        new OrderDto
                        {
                            Email = "alice@example.com",
                            Country = "US",
                            Payment = "card",
                            Total = 1200,
                            Items = new()
                            {
                                new ItemDto { Sku="NB-001", Qty=1, Stock=3 },
                                new ItemDto { Sku="KB-01",  Qty=2, Stock=10 }
                            }
                        }
                    }
                },
                new()
                {
                    Title = "Case 2 - Thiếu hàng & COD vượt hạn",
                    Orders = new()
                    {
                        new OrderDto
                        {
                            Email = "bob@shop.com",
                            Country = "US",
                            Payment = "cod",
                            Total = 350, // COD chỉ cho ≤ 200
                            Items = new()
                            {
                                new ItemDto { Sku="HD-1TB", Qty=2, Stock=0 } // hết hàng
                            }
                        }
                    }
                },
                new()
                {
                    Title = "Case 3 - Quốc gia không hỗ trợ & nghi gian lận",
                    Orders = new()
                    {
                        new OrderDto
                        {
                            Email = "fraud@badmail.com",
                            Country = "ZZ", // không hỗ trợ
                            Payment = "paypal",
                            Total = 9999,
                            Items = new()
                            {
                                new ItemDto { Sku="PH-09", Qty=1, Stock=5 }
                            }
                        }
                    }
                }
            };
            return Ok(ChainEcomExample.Run(demo));
        }
    }
}

//GET http://localhost:5102/api/chainecom/demo
//POST http://localhost:5102/api/chainecom/run
/*
[
  {
    "title": "Batch kiểm tra đơn",
    "orders": [
      {
        "email": "ok@buyer.com",
        "country": "VN",
        "payment": "cod",
        "total": 150,
        "items": [
          { "sku": "TS-01", "qty": 2, "stock": 5 }
        ]
      },
      {
        "email": "fraud@badmail.com",
        "country": "US",
        "payment": "card",
        "total": 500,
        "items": [
          { "sku": "NB-001", "qty": 1, "stock": 3 }
        ]
      },
      {
        "email": "big@order.com",
        "country": "US",
        "payment": "cod",
        "total": 12000,
        "items": [
          { "sku": "TV-70", "qty": 1, "stock": 2 }
        ]
      }
    ]
  }
]
*/
