using NetAgent.Abstractions.LLM;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.Models;

namespace NetAgent.Evaluation.Evaluators
{
    public class DefaultEvaluator : IEvaluator
    {
        private readonly IResponseScorer _responseScorer;

        public DefaultEvaluator(IResponseScorer responseScorer)
        {
            _responseScorer = responseScorer;
        }

        public async Task<EvaluationResult> EvaluateAsync(string prompt, string response, string goal, string context)
        {
            var score = _responseScorer.ScoreResponse(response);
            
            return new EvaluationResult 
            {
                Score = score,
                DetailedScores = new Dictionary<string, double> { ["ResponseScore"] = score }
            };
        }
    }
}
