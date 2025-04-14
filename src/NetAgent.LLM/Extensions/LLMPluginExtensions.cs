using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.Factory;
using System.Reflection;

namespace NetAgent.LLM.Extensions
{
    public static class LLMPluginExtensions 
    {
        public static IServiceCollection AddLLMPlugins(this IServiceCollection services)
        {
            // Register factory as singleton
            services.AddSingleton<LLMProviderFactory>();

            return services;
        }

        public static IServiceCollection ScanAndRegisterLLMPlugins(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            var factory = services.BuildServiceProvider().GetRequiredService<LLMProviderFactory>();

            // If no assemblies provided, scan entry assembly
            if (assemblies == null || assemblies.Length == 0)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    assemblies = new[] { entryAssembly };
                }
                else
                {
                    return services;
                }
            }

            // Find all types implementing ILLMProviderPlugin
            foreach (var assembly in assemblies)
            {
                var pluginTypes = assembly.GetTypes()
                    .Where(t => t != null && typeof(ILLMProviderPlugin).IsAssignableFrom(t) && !t.IsInterface);

                foreach (var pluginType in pluginTypes)
                {
                    // Create instance and register
                    var pluginInstance = Activator.CreateInstance(pluginType);
                    if (pluginInstance is ILLMProviderPlugin plugin)
                    {
                        factory.RegisterPlugin(plugin);

                        // Register configuration type if any
                        if (plugin.ConfigurationType != null)
                        {
                            // Register the options type with DI
                            services.AddOptions();

                            // Configure the options
                            var optionsBuilder = services.AddOptions<object>();
                            var optionsBuilderType = typeof(OptionsBuilder<>).MakeGenericType(plugin.ConfigurationType);
                            var configureOptionsMethod = typeof(OptionsBuilderExtensions)
                                .GetMethods()
                                .FirstOrDefault(m => m.Name == "Configure" && m.GetParameters().Length == 2)
                                ?.MakeGenericMethod(plugin.ConfigurationType);

                            if (configureOptionsMethod != null)
                            {
                                var defaultOptions = Activator.CreateInstance(plugin.ConfigurationType);
                                configureOptionsMethod.Invoke(null, new object[] 
                                { 
                                    optionsBuilder,
                                    new Action<object>(options => 
                                    {
                                        foreach (var prop in plugin.ConfigurationType.GetProperties())
                                        {
                                            var defaultValue = prop.GetValue(defaultOptions);
                                            prop.SetValue(options, defaultValue);
                                        }
                                    })
                                });
                            }
                        }
                    }
                }
            }

            return services;
        }
    }
}