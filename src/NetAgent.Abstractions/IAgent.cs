using NetAgent.Abstractions.Models;

namespace NetAgent.Abstractions
{
    public interface IAgent
    {
        Task<AgentResponse> ProcessAsync(AgentRequest request);
    }
}
