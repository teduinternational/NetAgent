using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.Tools;

namespace NetAgent.Tools.Standard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStandardAgentTools(this IServiceCollection services)
        {
            services.AddSingleton<IAgentTool, WebSearchTool>();
            services.AddSingleton<IAgentTool, CalculatorTool>();
            return services;
        }
    }

}
