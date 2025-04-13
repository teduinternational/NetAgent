using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.Models;

namespace NetAgent.Evaluation.Evaluators
{
    public class DummyEvaluator : IEvaluator
    {
        public Task<EvaluationResult> EvaluateAsync(string prompt, string output, string goal, string context)
        {
            return Task.FromResult(new EvaluationResult
            {
                Score = 1.0,
                IsAcceptable = true,
                Feedback = "Dummy always returns perfect score."
            });
        }

        public Task<bool> IsHealthyAsync()
        {
            return Task.FromResult(true);
        }
    }
}
