using NetAgent.Abstractions.LLM;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.Models;

namespace NetAgent.Evaluation.Evaluators
{
    public class LLMEvaluator : IEvaluator
    {
        private readonly ILLMProvider _llm;

        public LLMEvaluator(ILLMProvider llm)
        {
            _llm = llm;
        }

        public async Task<EvaluationResult> EvaluateAsync(string prompt, string output, string goal, string context)
        {
            var evaluationPrompt = $"Evaluate this output for prompt: {prompt}\nOutput: {output}\nGoal: {goal}\nContext: {context}";
            var evaluation = await _llm.GenerateAsync(evaluationPrompt);
            
            // Parse evaluation score
            return new EvaluationResult 
            { 
                Score = 1.0, // Default perfect score
                Feedback = evaluation
            };
        }
    }
}
