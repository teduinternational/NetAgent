using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.Optimization.Interfaces;
using NetAgent.Optimization.Models;
using System.Text.Json;

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
            var optimizationPrompt = @$"Analyze and optimize the following prompt to better achieve its goal.

                                        Original Prompt: {prompt}
                                        Goal: {goal}
                                        Context: {context}

                                        Please provide:
                                        1. An optimized version of the prompt that is more effective
                                        2. A list of specific improvements and suggestions

                                        Format your response as JSON:
                                        {{
                                            ""optimizedPrompt"": ""your optimized prompt here"",
                                            ""suggestions"": [""suggestion 1"", ""suggestion 2"", ...]
                                        }}";

            var response = await _llm.GenerateAsync(new Prompt()
            {
                Content = optimizationPrompt,
            });
            
            try
            {
                var result = JsonSerializer.Deserialize<OptimizationResult>(response.Content);
                return result ?? new OptimizationResult
                {
                    OptimizedPrompt = prompt,
                    Suggestions = new[] { "Failed to parse optimization response" }
                };
            }
            catch
            {
                return new OptimizationResult
                {
                    OptimizedPrompt = prompt,
                    Suggestions = new[] { "Error: Could not optimize the prompt" }
                };
            }
        }
    }
}
