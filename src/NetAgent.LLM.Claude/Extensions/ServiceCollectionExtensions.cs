using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.Claude.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClaudeAIProvider(this IServiceCollection services, ClaudeLLMOptions options)
        {
            services.AddSingleton(options);
            services.AddSingleton<ILLMProvider>(new ClaudeLLMProvider(options));
            return services;
        }
    }
}
