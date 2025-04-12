using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.OpenAI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenAIProvider(this IServiceCollection services, OpenAIOptions options)
        {
            services.AddSingleton<ILLMProvider>(new OpenAIProvider(options));
            return services;
        }
    }
}
