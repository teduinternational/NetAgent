using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.Tools;
using NetAgent.Tools.Standard.TavilySearch;

namespace NetAgent.Tools.Standard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStandardAgentTools(this IServiceCollection services)
        {
            services.AddSingleton<IAgentTool, TavilySearchTool>();
            return services;
        }
    }

}
