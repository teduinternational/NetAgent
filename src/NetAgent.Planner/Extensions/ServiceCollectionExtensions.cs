using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;
using NetAgent.Core.Planning;
using NetAgent.LLM.Claude;
using NetAgent.LLM.DeepSeek;
using NetAgent.LLM.Gemini;
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

        public static IServiceCollection AddClaudeAIProvider(this IServiceCollection services, ClaudeLLMOptions options)
        {
            services.AddSingleton<ILLMProvider>(new ClaudeLLMProvider(options));
            return services;
        }

        public static IServiceCollection AddDeepSeekProvider(this IServiceCollection services, DeepSeekOptions options)
        {
            services.AddSingleton<ILLMProvider>(new DeepSeekLLMProvider(options));
            return services;
        }

        public static IServiceCollection AddGrokProvider(this IServiceCollection services, GeminiOptions options)
        {
            services.AddSingleton<ILLMProvider>(new GeminiLLMProvider(options));
            return services;
        }
    }
}
