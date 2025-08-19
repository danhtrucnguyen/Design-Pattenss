using Microsoft.AspNetCore.Mvc;
using WebApplication1.BehavioralPatterns.Interpreter;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterpreterController : ControllerBase
    {
        // POST /api/interpreter/parse
        [HttpPost("parse")]
        public IActionResult Parse([FromBody] InterpreterJobDto job)
            => Ok(InterpreterExample.Run(job));

        // GET /api/interpreter/demo
        [HttpGet("demo")]
        public IActionResult Demo()
        {
            var job = new InterpreterJobDto
            {
                Romans = new() { "III", "IX", "LVIII", "MCMXCIV", "XLII", "MMXXV" }
            };
            return Ok(InterpreterExample.Run(job));
        }
    }
}
