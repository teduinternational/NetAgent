using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.OpenAI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenAIProvider(this IServiceCollection services, OpenAIOptions options)
        {
            // Register the options
            services.AddSingleton(options);
            
            // Register the plugin
            services.AddSingleton<ILLMProviderPlugin, OpenAIProviderPlugin>();
            
            return services;
        }
    }
}
