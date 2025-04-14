using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.Caching;
using NetAgent.LLM.Monitoring;
using NetAgent.LLM.RateLimiting;
using System.Collections.Concurrent;

namespace NetAgent.LLM.Factory
{
    public class LLMProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, ILLMProviderPlugin> _plugins;

        public LLMProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _plugins = new ConcurrentDictionary<string, ILLMProviderPlugin>();
        }

        public void RegisterPlugin(ILLMProviderPlugin plugin)
        {
            if (!_plugins.TryAdd(plugin.Name, plugin))
            {
                throw new InvalidOperationException($"Plugin {plugin.Name} is already registered");
            }
        }

        public ILLMProvider CreateProvider(string providerName, object? configuration = null)
        {
            if (!_plugins.TryGetValue(providerName, out var plugin))
            {
                throw new InvalidOperationException($"Provider {providerName} is not registered");
            }

            var provider = plugin.CreateProvider(_serviceProvider);

            // Add decorators if needed
            provider = ApplyDecorators(provider);

            return provider;
        }

        private ILLMProvider ApplyDecorators(ILLMProvider provider)
        {
            // Add monitoring if available
            var metrics = _serviceProvider.GetService<ILLMMetricsCollector>();
            if (metrics != null)
            {
                provider = new MonitoringLLMProviderDecorator(provider, metrics);
            }

            // Add caching if available
            var cache = _serviceProvider.GetService<ILLMResponseCache>();
            if (cache != null)
            {
                provider = new CachingLLMProviderDecorator(provider, cache, metrics);
            }

            // Add rate limiting if available
            var rateLimiter = _serviceProvider.GetService<ILLMRateLimiter>();
            if (rateLimiter != null)
            {
                provider = new RateLimitingLLMProviderDecorator(provider, rateLimiter, metrics);
            }

            return provider;
        }
    }
}
