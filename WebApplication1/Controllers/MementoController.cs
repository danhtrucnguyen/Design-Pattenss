using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.Memento;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MementoController : ControllerBase
    {
        // POST http://localhost:5102/api/memento/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<MementoJobDto> jobs)
            => Ok(MementoExample.Run(jobs));

        // GET  http://localhost:5102/api/memento/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<MementoJobDto>
            {
                new()
                {
                    Title = "Undo/Redo giỏ hàng",
                    Actions = new()
                    {
                        new ActDto{ Type="add",    Sku="A001", Qty=2, UnitPrice=50 },
                        new ActDto{ Type="save",   Note="Trước khi áp coupon" },
                        new ActDto{ Type="coupon", Code="SALE10", Percent=0.10m },
                        new ActDto{ Type="add",    Sku="B002", Qty=1, UnitPrice=200 },
                        new ActDto{ Type="undo" },         // quay lại trước khi thêm B002
                        new ActDto{ Type="undo" },         // quay lại trước khi áp coupon
                        new ActDto{ Type="redo" }          // tiến lại trạng thái “sau khi áp coupon”
                    }
                }
            };
            return Ok(MementoExample.Run(sample));
        }
    }
}
