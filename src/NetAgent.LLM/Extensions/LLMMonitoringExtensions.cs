using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.Monitoring;
using System;
using System.Linq;

namespace NetAgent.LLM.Extensions
{
    public static class LLMMonitoringExtensions
    {
        public static IServiceCollection AddLLMMonitoring(
            this IServiceCollection services,
            Action<LLMMetricsOptions> configureOptions = null)
        {
            // Configure metrics options
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            // Register metrics collector
            services.AddSingleton<ILLMMetricsCollector, DefaultLLMMetricsCollector>();

            // Decorate existing LLM providers with monitoring
            services.Decorate<ILLMProvider, MonitoringLLMProviderDecorator>();

            return services;
        }

        private static IServiceCollection Decorate<TService, TDecorator>(
            this IServiceCollection services)
            where TDecorator : class, TService
            where TService : class
        {
            var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));

            if (wrappedDescriptor == null)
                throw new InvalidOperationException($"Service {typeof(TService).Name} is not registered");

            var objectFactory = ActivatorUtilities.CreateFactory(
                typeof(TDecorator),
                new[] { typeof(TService), typeof(ILLMMetricsCollector) });

            // Remove existing registration
            services.Remove(wrappedDescriptor);

            // Add decorated implementation
            services.Add(ServiceDescriptor.Describe(
                typeof(TService),
                sp => (TService)objectFactory(sp, new object[] { 
                    CreateInstance(sp, wrappedDescriptor),
                    sp.GetRequiredService<ILLMMetricsCollector>()
                }),
                wrappedDescriptor.Lifetime));

            return services;
        }

        private static object CreateInstance(
            this IServiceProvider services,
            ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
                return descriptor.ImplementationInstance;

            if (descriptor.ImplementationFactory != null)
                return descriptor.ImplementationFactory(services);

            return ActivatorUtilities.GetServiceOrCreateInstance(
                services,
                descriptor.ImplementationType);
        }
    }
}