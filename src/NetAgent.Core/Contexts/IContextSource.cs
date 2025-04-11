using NetAgent.Abstractions.Models;

namespace NetAgent.Core.Contexts
{
    public interface IContextSource
    {
        Task<AgentInputContext> GetContextAsync();
    }
}
