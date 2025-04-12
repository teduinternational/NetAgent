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
    }
}
