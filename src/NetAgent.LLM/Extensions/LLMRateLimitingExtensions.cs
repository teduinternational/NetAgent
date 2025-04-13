using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.RateLimiting;
using System;
using System.Linq;

namespace NetAgent.LLM.Extensions
{
    public static class LLMRateLimitingExtensions
    {
        public static IServiceCollection AddLLMRateLimiting(
            this IServiceCollection services,
            Action<RateLimitOptions> configureOptions = null)
        {
            // Configure rate limit options
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            // Register rate limiter
            services.AddSingleton<ILLMRateLimiter, DefaultLLMRateLimiter>();

            // Decorate LLM providers with rate limiting
            services.Decorate<ILLMProvider, RateLimitingLLMProviderDecorator>();

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
                new[] { typeof(TService), typeof(ILLMRateLimiter) });

            // Remove existing registration
            services.Remove(wrappedDescriptor);

            // Add decorated implementation
            services.Add(ServiceDescriptor.Describe(
                typeof(TService),
                sp => (TService)objectFactory(sp, new object[] { 
                    CreateInstance(sp, wrappedDescriptor),
                    sp.GetRequiredService<ILLMRateLimiter>()
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