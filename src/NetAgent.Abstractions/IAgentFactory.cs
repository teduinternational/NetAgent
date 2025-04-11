using NetAgent.Abstractions.Models;

namespace NetAgent.Abstractions
{
    public interface IAgentFactory
    {
        Task<IAgent> CreateAgent(AgentOptions options);
    }
}