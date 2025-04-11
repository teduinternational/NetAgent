using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;
using NetAgent.Core.Planning;
using NetAgent.LLM.AzureOpenAI;
using NetAgent.LLM.Ollama;
using NetAgent.LLM.OpenAI;
using NetAgent.Planner.Default;

namespace NetAgent.Planner.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultPlanner(this IServiceCollection services)
        {
            services.AddSingleton<IAgentPlanner, DefaultPlanner>();
            return services;
        }

        public static IServiceCollection AddOpenAIProvider(this IServiceCollection services, OpenAIOptions options)
        {
            services.AddSingleton<ILLMProvider>(new OpenAIProvider(options));
            return services;
        }

        public static IServiceCollection AddAzureOpenAIProvider(this IServiceCollection services, AzureOpenAIOptions options)
        {
            services.AddSingleton<ILLMProvider>(new AzureOpenAIProvider(options));
            return services;
        }

        public static IServiceCollection AddOllamaProvider(this IServiceCollection services, OllamaOptions options)
        {
            services.AddSingleton<ILLMProvider>(new OllamaProvider(options));
            return services;
        }
    }
}
