using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.Providers;
using NetAgent.LLM.Scoring;
using System;
using System.Linq;
using System.Reflection;

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

        public static IServiceCollection AddLLMProviderPlugin<TPlugin>(
            this IServiceCollection services,
            object configuration = null)
            where TPlugin : class, ILLMProviderPlugin
        {
            // Register the plugin
            services.AddSingleton<ILLMProviderPlugin, TPlugin>();

            // If configuration is provided, register it
            if (configuration != null)
            {
                var plugin = ActivatorUtilities.CreateInstance<TPlugin>(services.BuildServiceProvider());
                var configType = plugin.ConfigurationType;
                
                // Use the correct method to configure options
                var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                    .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure))
                    ?.MakeGenericMethod(configType);
                
                configureMethod?.Invoke(null, new object[] { services, null, configuration });
            }

            return services;
        }

        public static IServiceCollection ScanAndRegisterLLMPlugins(
            this IServiceCollection services,
            Assembly assembly = null)
        {
            var assemblies = assembly != null 
                ? new[] { assembly } 
                : AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                var pluginTypes = asm.GetTypes()
                    .Where(t => !t.IsAbstract && typeof(ILLMProviderPlugin).IsAssignableFrom(t));

                foreach (var pluginType in pluginTypes)
                {
                    services.AddSingleton(typeof(ILLMProviderPlugin), pluginType);
                }
            }

            return services;
        }
    }
}
