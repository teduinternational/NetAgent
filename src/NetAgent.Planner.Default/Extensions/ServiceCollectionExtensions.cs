using Microsoft.Extensions.DependencyInjection;
using NetAgent.Core.Planning;
using NetAgent.LLM.AzureOpenAI;
using NetAgent.LLM.Interfaces;
using NetAgent.LLM.Ollama;
using NetAgent.LLM.OpenAI;

namespace NetAgent.Planner.Default.Extensions
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
