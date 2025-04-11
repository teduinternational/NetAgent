using NetAgent.Abstractions.LLM;
using NetAgent.Optimization.Interfaces;
using NetAgent.Optimization.Models;
using System.Threading.Tasks;

namespace NetAgent.Optimization.Optimizers
{
    public class PromptOptimizer : IOptimizer
    {
        private readonly ILLMProvider _llm;

        public PromptOptimizer(ILLMProvider llm)
        {
            _llm = llm;
        }


        public async Task<OptimizationResult> OptimizeAsync(string prompt, string goal, string context)
        {
            var newPrompt = $"Optimize this prompt for better results:\n{context}";
            var optimizedPrompt = await _llm.GenerateAsync(newPrompt);
            return new OptimizationResult
            {
                OptimizedPrompt = optimizedPrompt,
                Suggestions = new[] { "No optimization applied (dummy)" }
            };
        }
    }
}
