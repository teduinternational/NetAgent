using NetAgent.Optimization.Models;

namespace NetAgent.Optimization.Interfaces
{
    public interface IOptimizer
    {
        Task<OptimizationResult> OptimizeAsync(string prompt, string goal, string context);
    }
}
