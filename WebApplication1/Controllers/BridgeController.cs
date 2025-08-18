using Design_Patterns.StructuralPatterns.Bridge;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApplication1.StructuralPatterns.Bridge;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BridgeController : ControllerBase
    {
        // POST /api/bridge/export  (nhận danh sách job và trả kết quả từng job)
        [HttpPost("export")]
        public IActionResult Export([FromBody] List<ReportJobDto> jobs)
        {
            var result = BridgeExample.Run(jobs);
            return Ok(result);
        }

        // GET /api/bridge/demo  ( dữ liệu mẫu )
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<ReportJobDto>
            {
                new()
                {
                    Type = "sales",
                    Renderer = "html",
                    CurrencyCulture = "en-US",
                    SalesData = new List<decimal> { 100m, 250.50m, 75m }
                },
                new()
                {
                    Type = "sales",
                    Renderer = "pdf",
                    CurrencyCulture = "vi-VN",
                    SalesData = new List<decimal> { 1200000m, 350000m }
                },
                new()
                {
                    Type = "inventory",
                    Renderer = "html",
                    Stock = new Dictionary<string,int> { ["A001"]=50, ["B002"]=20, ["C003"]=0 }
                },
                new()
                {
                    Type = "inventory",
                    Renderer = "pdf",
                    Stock = new Dictionary<string,int> { ["X01"]=5, ["Y02"]=7 }
                }
            };

            var result = BridgeExample.Run(sample);
            return Ok(result);
        }
    }
}
