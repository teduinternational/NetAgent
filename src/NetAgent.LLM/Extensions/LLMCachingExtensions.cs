using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.Caching;

namespace NetAgent.LLM.Extensions
{
    public static class LLMCachingExtensions
    {
        public static IServiceCollection AddLLMCaching(
            this IServiceCollection services,
            Action<LLMCacheOptions> configureOptions = null)
        {
            // Configure cache options
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            // Register cache service
            services.AddSingleton<ILLMResponseCache, MemoryLLMResponseCache>();

            // Decorate existing LLM providers with caching
            services.Decorate<ILLMProvider, CachingLLMProviderDecorator>();

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
                new[] { typeof(TService), typeof(ILLMResponseCache) });

            // Remove existing registration
            services.Remove(wrappedDescriptor);

            // Add decorated implementation
            services.Add(ServiceDescriptor.Describe(
                typeof(TService),
                sp => (TService)objectFactory(sp, new object[] { 
                    CreateInstance(sp, wrappedDescriptor),
                    sp.GetRequiredService<ILLMResponseCache>()
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