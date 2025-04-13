using NetAgent.Optimization.Models;
using NetAgent.LLM.Monitoring;

namespace NetAgent.Optimization.Interfaces
{
    public interface IOptimizer
    {
        Task<bool> IsHealthyAsync();
        Task<OptimizationResult> OptimizeAsync(string prompt, string goal, string context);
    }
}
