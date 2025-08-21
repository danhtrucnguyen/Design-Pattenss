using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.State;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StateController : ControllerBase
    {
        // POST http://localhost:5102/api/state/run
        [HttpPost("run")]
        public IActionResult Run([FromBody] List<StateJobDto> jobs)
            => Ok(StateExample.Run(jobs));

        // GET http://localhost:5102/api/state/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var sample = new List<StateJobDto>
            {
                new()
                {
                    Title = "Flow cơ bản",
                    Actions = new()
                    {
                        new ActionDto{ Type="add", Amount=50, Qty=2 },
                        new ActionDto{ Type="add", Amount=200, Qty=1 },
                        new ActionDto{ Type="pay" },
                        new ActionDto{ Type="ship" },
                        new ActionDto{ Type="deliver" }
                    }
                },
                new()
                {
                    Title = "Hủy trước khi ship",
                    Actions = new()
                    {
                        new ActionDto{ Type="add", Amount=100, Qty=1 },
                        new ActionDto{ Type="pay" },
                        new ActionDto{ Type="cancel" }
                    }
                },
                new()
                {
                    Title = "Lỗi thao tác",
                    Actions = new()
                    {
                        new ActionDto{ Type="ship" },              // ship khi còn Cart → bị từ chối
                        new ActionDto{ Type="add", Amount=10, Qty=1 },
                        new ActionDto{ Type="pay" },
                        new ActionDto{ Type="add", Amount=5, Qty=1 } // add sau khi pay → bị từ chối
                    }
                }
            };
            return Ok(StateExample.Run(sample));
        }
    }
}
