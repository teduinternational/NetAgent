using NetAgent.LLM.Interfaces;
using NetAgent.Optimization.Interfaces;
using NetAgent.Optimization.Models;

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
            var optimizePrompt = $"""
                You are a prompt engineer. Improve the prompt for better performance in achieving the goal.

                Goal: {goal}
                Context: {context}
                Original Prompt: {prompt}

                Return improved prompt followed by suggestions.
                Format:
                Optimized Prompt: <optimized>
                Suggestions: <comma separated suggestions>
            """;

            var response = await _llm.GenerateAsync(optimizePrompt);

            var optimizedPrompt = Extract(response, "Optimized Prompt:");
            var suggestions = Extract(response, "Suggestions:")
                .Split(',').Select(x => x.Trim()).ToArray();

            return new OptimizationResult
            {
                OptimizedPrompt = optimizedPrompt,
                Suggestions = suggestions
            };
        }

        private static string Extract(string input, string label)
        {
            var index = input.IndexOf(label, StringComparison.OrdinalIgnoreCase);
            if (index == -1) return string.Empty;
            var rest = input.Substring(index + label.Length).Trim();
            var nextLabelIndex = rest.IndexOf(':');
            return nextLabelIndex != -1 ? rest.Substring(0, nextLabelIndex).Trim() : rest;
        }
    }
}
