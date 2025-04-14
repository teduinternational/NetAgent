using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.Models;
using NetAgent.LLM.Monitoring;

namespace NetAgent.Evaluation.Evaluators
{
    public class DefaultEvaluator : IEvaluator
    {
        private readonly ILLMProvider _llm;
        private readonly ILLMHealthCheck? _healthCheck;
        private readonly IResponseScorer? _responseScorer;
        private const double ACCEPTABLE_THRESHOLD = 0.7;

        public DefaultEvaluator(
            ILLMProvider llm, 
            ILLMHealthCheck? healthCheck = null,
            IResponseScorer? responseScorer = null)
        {
            _llm = llm;
            _healthCheck = healthCheck;
            _responseScorer = responseScorer;
        }

        public async Task<bool> IsHealthyAsync()
        {
            if (_healthCheck == null)
                return true;

            var result = await _healthCheck.CheckHealthAsync(_llm.Name);
            return result.Status == HealthStatus.Healthy;
        }

        public async Task<EvaluationResult> EvaluateAsync(string prompt, string output, string goal, string context)
        {
            var evaluationPrompt = $"""
                Evaluate this output for prompt: {prompt}
                Output: {output}
                Goal: {goal}
                Context: {context}
                Provide evaluation score between 0.0 and 1.0, where 1.0 is perfect. Format your response as a JSON with score and feedback fields.
                """;

            var llmResponse = await _llm.GenerateAsync(new Prompt { Content = evaluationPrompt });

            if (llmResponse.IsError)
            {
                return new EvaluationResult
                {
                    IsError = true,
                    Feedback = "Evaluation was not performed as LLM was not called."
                };
            }
            
            try
            {
                // Try to parse JSON response
                var jsonResponse = System.Text.Json.JsonDocument.Parse(llmResponse.Content);
                var score = jsonResponse.RootElement.GetProperty("score").GetDouble();
                var feedback = jsonResponse.RootElement.GetProperty("feedback").GetString() ?? llmResponse.Content;

                // Apply additional scoring if response scorer is available
                if (_responseScorer != null)
                {
                    var responseScore = _responseScorer.ScoreResponse(output);
                    // Combine both scores with equal weight
                    score = (score + responseScore) / 2;
                }

                return new EvaluationResult 
                { 
                    Score = score,
                    IsAcceptable = score >= ACCEPTABLE_THRESHOLD,
                    Feedback = feedback
                };
            }
            catch
            {
                // Fallback to basic parsing if JSON parsing fails
                double score = _responseScorer?.ScoreResponse(output) ?? 1.0;
                if (!double.TryParse(llmResponse.Content, out var llmScore))
                {
                    llmScore = score;
                }
                score = (score + llmScore) / 2;
                
                return new EvaluationResult 
                { 
                    Score = score,
                    IsAcceptable = score >= ACCEPTABLE_THRESHOLD,
                    Feedback = llmResponse.Content
                };
            }
        }
    }
}
