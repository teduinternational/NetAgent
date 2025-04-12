using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.DeepSeek.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeepSeekProvider(this IServiceCollection services, DeepSeekOptions options)
        {
            services.AddSingleton<ILLMProvider>(new DeepSeekLLMProvider(options));
            return services;
        }
    }
}
