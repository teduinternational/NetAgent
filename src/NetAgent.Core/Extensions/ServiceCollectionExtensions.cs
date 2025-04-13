using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions;
using NetAgent.Abstractions.Tools;
using NetAgent.Abstractions.LLM;
using NetAgent.Core.Configuration;
using NetAgent.Core.Tools;
using NetAgent.Abstractions.Models;

namespace NetAgent.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNetAgentCore(this IServiceCollection services)
        {
            // Add core services
            services.AddSingleton<IAgentConfiguration, DefaultAgentConfiguration>();
            services.AddSingleton<IToolRegistry, DefaultToolRegistry>();

            return services;
        }

        public static IServiceCollection AddNetAgentTools(this IServiceCollection services, Action<ToolOptions>? configureTools = null)
        {
            var toolOptions = new ToolOptions
            {
                EnabledTools = new List<string>(),
                ToolSettings = new Dictionary<string, object>()
            };

            configureTools?.Invoke(toolOptions);

            services.Configure<ToolOptions>(options =>
            {
                options.EnabledTools = toolOptions.EnabledTools;
                options.ToolSettings = toolOptions.ToolSettings;
            });

            return services;
        }

        public static IServiceCollection AddLLMProvider<TProvider, TOptions>(
            this IServiceCollection services,
            Action<TOptions> configureOptions) 
            where TProvider : class, ILLMProvider
            where TOptions : class, new()
        {
            services.Configure(configureOptions);
            services.AddSingleton<ILLMProvider, TProvider>();

            return services;
        }
    }
}