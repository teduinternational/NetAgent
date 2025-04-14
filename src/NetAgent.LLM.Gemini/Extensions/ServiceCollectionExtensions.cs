using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.Gemini.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGeminiProvider(this IServiceCollection services, GeminiOptions options)
        {
            // Register the options
            services.AddSingleton(options);
            
            // Register the plugin
            services.AddSingleton<ILLMProviderPlugin, GeminiLLMProviderPlugin>();
            
            return services;
        }
    }
}
