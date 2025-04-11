using NetAgent.Abstractions.Models;

namespace NetAgent.Strategy.Strategies
{
    public class GoalDrivenStrategy : IAgentStrategy
    {
        public Task<AgentDecision> DecideNextStepAsync(string goal, AgentInputContext context)
        {
            return Task.FromResult(new AgentDecision
            {
                Plan = $"Analyze goal: {goal} and use general tools.",
                ToolToUse = null,
                SkipToolExecution = false
            });
        }
    }
}
