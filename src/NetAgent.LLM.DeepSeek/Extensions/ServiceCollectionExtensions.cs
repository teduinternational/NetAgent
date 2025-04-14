using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.DeepSeek.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeepSeekProvider(this IServiceCollection services, DeepSeekOptions options)
        {
            // Register the options
            services.AddSingleton(options);
            
            // Register the plugin
            services.AddSingleton<ILLMProviderPlugin, DeepSeekLLMProviderPlugin>();
            
            return services;
        }
    }
}
