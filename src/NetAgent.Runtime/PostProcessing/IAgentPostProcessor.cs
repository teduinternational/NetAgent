using NetAgent.Abstractions.Models;

namespace NetAgent.Runtime.PostProcessing
{
    public interface IAgentPostProcessor
    {
        Task PostProcessAsync(AgentResponse response, AgentInputContext context);
    }
}
