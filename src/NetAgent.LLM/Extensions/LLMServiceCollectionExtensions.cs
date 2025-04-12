using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.Providers;
using NetAgent.LLM.Scoring;

namespace NetAgent.LLM.Extensions
{
    public static class LLMServiceCollectionExtensions
    {
        public static IServiceCollection AddMultiLLMProviders(this IServiceCollection services)
        {
            services.AddSingleton<IMultiLLMProvider>(sp =>
            {
                var providers = sp.GetServices<ILLMProvider>();
                var scorer = sp.GetRequiredService<IResponseScorer>();
                var logger = sp.GetRequiredService<ILogger<MultiLLMProvider>>();
                return new MultiLLMProvider(providers, scorer, logger);
            });
            services.AddSingleton<IResponseScorer, DefaultResponseScorer>();

            return services;
        }
    }
}
