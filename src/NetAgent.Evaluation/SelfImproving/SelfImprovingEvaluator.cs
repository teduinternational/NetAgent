using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.Models;
using NetAgent.LLM.Monitoring;

namespace NetAgent.Evaluation.SelfImproving
{
    public class SelfImprovingEvaluator : IEvaluator
    {
        private readonly ILLMProvider _llmProvider;
        private readonly ILLMHealthCheck _healthCheck;
        private const double ACCEPTABLE_THRESHOLD = 0.7;
        private const int MAX_RETRIES = 3;

        public SelfImprovingEvaluator(ILLMProvider llmProvider, ILLMHealthCheck healthCheck)
        {
            _llmProvider = llmProvider;
            _healthCheck = healthCheck;
        }

        public async Task<bool> IsHealthyAsync()
        {
            var healthResult = await _healthCheck.CheckHealthAsync(_llmProvider.Name);
            return healthResult.Status == HealthStatus.Healthy;
        }

        public async Task<EvaluationResult> EvaluateAsync(string prompt, string response, string goal, string context)
        {
            // First check health status
            if (!await IsHealthyAsync())
            {
                throw new InvalidOperationException($"LLM Provider {_llmProvider.Name} is not healthy");
            }

            var evaluationPrompt = $"""
                Act as an expert evaluator. Analyze the following output:
                
                Original Prompt: {prompt}
                Response to Evaluate: {response}
                Goal: {goal}
                Context: {context}

                Evaluate based on:
                1. Relevance to prompt and goal
                2. Accuracy and correctness
                3. Completeness
                4. Quality and clarity

                Provide your evaluation as JSON with:
                - score (0.0 to 1.0, where 1.0 is perfect)
                - feedback (detailed explanation)
                - improvements (list of suggested improvements)
                """;

            EvaluationResult? result = null;
            Exception? lastException = null;

            // Implement retry logic with exponential backoff
            for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
            {
                try
                {
                    var evaluation = await _llmProvider.GenerateAsync(new Prompt { Content = evaluationPrompt });
                    var jsonResponse = System.Text.Json.JsonDocument.Parse(evaluation.Content);
                    
                    var score = jsonResponse.RootElement.GetProperty("score").GetDouble();
                    var feedback = jsonResponse.RootElement.GetProperty("feedback").GetString() ?? string.Empty;
                    var improvements = new List<string>();

                    if (jsonResponse.RootElement.TryGetProperty("improvements", out var improvementsElement))
                    {
                        foreach (var improvement in improvementsElement.EnumerateArray())
                        {
                            improvements.Add(improvement.GetString() ?? string.Empty);
                        }
                    }

                    result = new EvaluationResult
                    {
                        Score = score,
                        IsAcceptable = score >= ACCEPTABLE_THRESHOLD,
                        Feedback = feedback,
                        Improvements = improvements
                    };

                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    await Task.Delay((1 << attempt) * 1000); // Exponential backoff
                }
            }

            if (result == null)
            {
                throw new InvalidOperationException("Failed to evaluate after multiple attempts", lastException);
            }

            return result;
        }
    }
}