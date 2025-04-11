using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.Providers;

namespace NetAgent.LLM.Extensions
{
    public static class LLMServiceCollectionExtensions
    {
        public static IServiceCollection AddMultiLLMProvider(this IServiceCollection services)
        {
            services.AddTransient<IMultiLLMProvider, MultiLLMProvider>();
            return services;
        }
    }
}
