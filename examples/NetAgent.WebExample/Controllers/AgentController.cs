using Microsoft.AspNetCore.Mvc;
using NetAgent.Abstractions;
using NetAgent.Abstractions.Models;
using NetAgent.WebExample.Models;

namespace NetAgent.WebExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgent _agent;

        public AgentController(IAgent agent)
        {
            _agent = agent;
        }

        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] GoalRequest request)
        {
            var result = await _agent.ProcessAsync(new AgentRequest()
            {
                Goal = request.Goal
            });
            return Ok(new { Result = result });
        }
    }
}
