using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.Evaluation.Evaluators;
using NetAgent.Evaluation.Interfaces;

namespace NetAgent.Evaluation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEvaluatorFromConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var evaluatorType = configuration["NetAgent:Evaluator:Type"]?.ToLowerInvariant() ?? "llm";

            return evaluatorType switch
            {
                "llm" => services.AddSingleton<IEvaluator, LLMEvaluator>(),
                "dummy" => services.AddSingleton<IEvaluator, DummyEvaluator>(),
                _ => throw new InvalidOperationException($"Unknown evaluator type: {evaluatorType}")
            };
        }
    }
}
