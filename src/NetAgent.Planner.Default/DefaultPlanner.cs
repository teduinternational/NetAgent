using NetAgent.Abstractions.Models;
using NetAgent.Core.Planning;

namespace NetAgent.Planner.Default
{
    public class DefaultPlanner : IAgentPlanner
    {
        public Task<string> PlanNextStepAsync(string goal, AgentInputContext context)
        {
            // TODO: Nâng cấp dùng LLM hoặc rule engine nếu cần
            var plan = $"Analyze: {goal} with current context.";
            return Task.FromResult(plan);
        }
    }
}
