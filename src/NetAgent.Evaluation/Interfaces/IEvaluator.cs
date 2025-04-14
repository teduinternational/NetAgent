using NetAgent.Evaluation.Models;

namespace NetAgent.Evaluation.Interfaces
{
    public interface IEvaluator
    {
        Task<bool> IsHealthyAsync();
        Task<EvaluationResult> EvaluateAsync(string prompt, string response, string goal, string context);
    }
}
