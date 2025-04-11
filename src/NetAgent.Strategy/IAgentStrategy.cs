using NetAgent.Abstractions.Models;

namespace NetAgent.Strategy
{
    public interface IAgentStrategy
    {
        Task<AgentDecision> DecideNextStepAsync(string goal, AgentInputContext context);
    }
}
