using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.Optimization.Interfaces;
using NetAgent.Optimization.Models;
using NetAgent.LLM.Monitoring;
using System.Text.Json;

namespace NetAgent.Optimization.Optimizers
{
    public class DefaultOptimizer : IOptimizer
    {
        private readonly ILLMProvider _llmProvider;
        private readonly ILLMHealthCheck _healthCheck;

        public DefaultOptimizer(ILLMProvider llmProvider, ILLMHealthCheck healthCheck)
        {
            _llmProvider = llmProvider;
            _healthCheck = healthCheck;
        }

        public async Task<bool> IsHealthyAsync()
        {
            var healthResult = await _healthCheck.CheckHealthAsync(_llmProvider.Name);
            return healthResult.Status == HealthStatus.Healthy;
        }

        public async Task<OptimizationResult> OptimizeAsync(string prompt, string goal, string context)
        {
            if (!await IsHealthyAsync())
            {
                return new OptimizationResult()
                {
                    IsError = true,
                    OptimizedPrompt = "Provider is unhealthy, unable to generate response."
                };
            }

            var optimizationPrompt = @$"As an AI optimizer, analyze and enhance the following prompt while considering its goal and context. Apply self-improving techniques to generate an optimal version.

                                      Original Prompt: {prompt}
                                      Goal: {goal}
                                      Context: {context}

                                      Provide:
                                      1. A significantly improved version of the prompt
                                      2. Detailed reasoning for improvements
                                      3. Learning-based suggestions for future optimizations

                                      Format response as JSON:
                                      {{
                                          ""optimizedPrompt"": ""enhanced prompt here"",
                                          ""suggestions"": [
                                              ""improvement reasoning 1"",
                                              ""learning suggestion 1"",
                                              ""future optimization tip 1""
                                          ]
                                      }}";

            try
            {
                var response = await _llmProvider.GenerateAsync(new Prompt 
                { 
                    Content = optimizationPrompt
                });

                var result = JsonSerializer.Deserialize<OptimizationResult>(response.Content);
                if (result != null)
                {
                    return result;
                }
            }
            catch
            {
                // Silently handle exceptions and use fallback
            }

            // Fallback if optimization fails
            return new OptimizationResult
            {
                OptimizedPrompt = prompt,
                Suggestions = new[] { 
                    "Self-improving optimization failed",
                    "Using original prompt as fallback",
                    "Consider reviewing LLM provider settings"
                }
            };
        }
    }
}