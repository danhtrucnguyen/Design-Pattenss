using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WebApplication1.StructuralPatterns.Proxy;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyController : ControllerBase
    {
        // POST /api/proxy/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<InventoryJobDto> jobs)
            => Ok(ProxyExample.Run(jobs));

        // GET /api/proxy/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<InventoryJobDto>
            {
                // Job 1: Lazy + Cache + có quyền ghi
                new()
                {
                    InitialStock   = new() { ["SKU-001"] = 10, ["SKU-003"] = 7 },
                    LatencyMs      = 400,              // mô phỏng chậm
                    UseLazy        = true,             // chỉ tạo RealInventory khi cần
                    UseCaching     = true,
                    TtlMs          = 1500,             // TTL cache 1.5s
                    UseProtection  = false,
                    CanWrite       = true,
                    Commands = new()
                    {
                        new() { Action = "get",     Sku = "SKU-001" },       // miss -> gọi real
                        new() { Action = "get",     Sku = "SKU-001" },       // hit cache
                        new() { Action = "add",     Sku = "SKU-001", Qty = 3 },
                        new() { Action = "get",     Sku = "SKU-001" },       // miss do invalidated
                        new() { Action = "wait",    DelayMs = 1600 },        // chờ quá TTL
                        new() { Action = "get",     Sku = "SKU-001" }        // miss vì TTL
                    }
                },
                // Job 2: Protection proxy chặn ghi
                new()
                {
                    InitialStock   = new() { ["SKU-002"] = 5 },
                    LatencyMs      = 100,
                    UseLazy        = false,
                    UseCaching     = false,
                    UseProtection  = true,
                    CanWrite       = false,           // không cho ghi
                    Commands = new()
                    {
                        new() { Action = "get",     Sku = "SKU-002" },
                        new() { Action = "add",     Sku = "SKU-002", Qty = 3 }, // sẽ bị chặn
                        new() { Action = "reserve", Sku = "SKU-002", Qty = 2 }  // sẽ bị chặn
                    }
                }
            };

            return Ok(ProxyExample.Run(sample));
        }
    }
}

//GET http://localhost:5102/api/proxy/demo
//POST http://localhost:5102/api/proxy/run

/*
[
  {
    "initialStock": { "SKU-001": 10, "SKU-003": 7 },
    "latencyMs": 400,
    "useLazy": true,
    "useCaching": true,
    "ttlMs": 1500,
    "useProtection": false,
    "canWrite": true,
    "commands": [
      { "action": "get",     "sku": "SKU-001" },
      { "action": "get",     "sku": "SKU-001" },
      { "action": "add",     "sku": "SKU-001", "qty": 3 },
      { "action": "get",     "sku": "SKU-001" },
      { "action": "wait",    "delayMs": 1600 },
      { "action": "get",     "sku": "SKU-001" }
    ]
  },
  {
    "initialStock": { "SKU-002": 5 },
    "latencyMs": 100,
    "useLazy": false,
    "useCaching": false,
    "useProtection": true,
    "canWrite": false,
    "commands": [
      { "action": "get",     "sku": "SKU-002" },
      { "action": "add",     "sku": "SKU-002", "qty": 3 },
      { "action": "reserve", "sku": "SKU-002", "qty": 2 }
    ]
  }
]
*/