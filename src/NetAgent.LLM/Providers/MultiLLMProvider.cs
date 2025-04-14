﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;

namespace NetAgent.LLM.Providers
{
    public class MultiLLMProvider : IMultiLLMProvider
    {
        private readonly IEnumerable<ILLMProvider> _providers;
        private readonly IResponseScorer _scorer;
        private readonly ILogger<IMultiLLMProvider> _logger;
        private readonly ConcurrentDictionary<string, DateTime> _failedProviders = new();
        private readonly ILLMPreferences _preferences;
        private const int RETRY_MINUTES = 5;

        public MultiLLMProvider(
            IEnumerable<ILLMProvider> providers,
            IResponseScorer scorer,
            ILogger<IMultiLLMProvider> logger,
            ILLMPreferences? preferences = null)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
            _scorer = scorer ?? throw new ArgumentNullException(nameof(scorer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // If no preferences provided, create a default one that allows all providers
            _preferences = preferences ?? new DefaultLLMPreferences(); 
        }

        private class DefaultLLMPreferences : ILLMPreferences
        {
            public IEnumerable<string> PreferredProviders => throw new NotImplementedException();

            public bool IsProviderAllowed(string providerName) => true;
            public double GetProviderWeight(string providerName) => 1.0;
        }

        public string Name => "MultiLLM";

        public async Task<LLMResponse[]> GenerateFromAllAsync(Prompt prompt)
        {
            var results = new List<LLMResponse>();
            var orderedProviders = OrderProvidersByPreference(_providers);

            foreach (var provider in orderedProviders)
            {
                if (!await IsProviderAvailableAndHealthyAsync(provider))
                    continue;

                try
                {
                    var result = await provider.GenerateAsync(prompt);
                    results.Add(result);
                    _failedProviders.TryRemove(provider.Name, out _);
                }
                catch (LLMException ex)
                {
                    _logger.LogWarning("Provider {Provider} failed: {Message}", provider.Name, ex.Message);
                    HandleProviderFailure(provider.Name);
                }
            }

            if (!results.Any())
                throw new LLMException("All available LLM providers failed or are temporarily disabled");

            return results.ToArray();
        }

        public async Task<LLMResponse> GenerateBestAsync(Prompt prompt)
        {
            var results = await GenerateFromAllAsync(prompt);
            return results
                .OrderByDescending(r => _scorer.ScoreResponse(r.Content))
                .FirstOrDefault() ?? new LLMResponse();
        }

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            var orderedProviders = OrderProvidersByPreference(_providers).ToList();
            var availableProviders = (await Task.WhenAll(orderedProviders.Select(async p => 
                new { Provider = p, IsAvailable = await IsProviderAvailableAndHealthyAsync(p) })))
                .Where(p => p.IsAvailable)
                .Select(p => p.Provider)
                .ToList();

            var exceptions = new List<Exception>();

            if (!availableProviders.Any())
            {
                _logger.LogWarning("No available healthy providers found");
                throw new LLMException("All available LLM providers failed, are unhealthy, or are temporarily disabled");
            }

            foreach (var provider in availableProviders)
            {
                try
                {
                    var result = await provider.GenerateAsync(prompt);
                    _failedProviders.TryRemove(provider.Name, out _);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Provider {Provider} failed: {Message}", provider.Name, ex.Message);
                    HandleProviderFailure(provider.Name);
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException("All available LLM providers failed", exceptions);
        }

        public IEnumerable<ILLMProvider> GetProviders()
        {
            return _providers;
        }

        public IResponseScorer GetScorer()
        {
            return _scorer;
        }

        public ILogger<IMultiLLMProvider> GetLogger()
        {
            return _logger;
        }

        private IEnumerable<ILLMProvider> OrderProvidersByPreference(IEnumerable<ILLMProvider> providers)
        {
            if (_preferences == null)
                return providers;

            return providers
                .Where(p => _preferences.IsProviderAllowed(p.Name))
                .OrderByDescending(p => _preferences.GetProviderWeight(p.Name));
        }

        private bool IsProviderAvailable(ILLMProvider provider)
        {
            if (_preferences?.IsProviderAllowed(provider.Name) == false)
            {
                _logger.LogDebug("Provider {Provider} is not allowed by preferences", provider.Name);
                return false;
            }

            if (IsProviderTemporarilyDisabled(provider.Name))
            {
                _logger.LogDebug("Provider {Provider} is temporarily disabled", provider.Name);
                return false;
            }

            return true;
        }

        private bool IsProviderTemporarilyDisabled(string providerName)
        {
            if (_failedProviders.TryGetValue(providerName, out var failedTime))
            {
                var timeSinceFailure = DateTime.UtcNow - failedTime;
                if (timeSinceFailure < TimeSpan.FromMinutes(RETRY_MINUTES))
                {
                    _logger.LogDebug("Provider {Provider} is temporarily disabled for {RemainingTime} minutes", 
                        providerName, 
                        (TimeSpan.FromMinutes(RETRY_MINUTES) - timeSinceFailure).TotalMinutes);
                    return true;
                }
                
                _failedProviders.TryRemove(providerName, out _);
            }
            return false;
        }

        private void HandleProviderFailure(string providerName)
        {
            _failedProviders.AddOrUpdate(
                providerName,
                DateTime.UtcNow,
                (_, _) => DateTime.UtcNow
            );
            _logger.LogWarning("Provider {Provider} marked as failed at {Time}", providerName, DateTime.UtcNow);
        }

        private async Task<bool> IsProviderAvailableAndHealthyAsync(ILLMProvider provider)
        {
            if (!IsProviderAvailable(provider))
                return false;

            try
            {
                return await provider.IsHealthyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Health check failed for provider {Provider}: {Message}", provider.Name, ex.Message);
                return false;
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            var providers = OrderProvidersByPreference(_providers);
            var healthyProviders = await Task.WhenAll(
                providers.Select(async p => await IsProviderAvailableAndHealthyAsync(p)));
            
            return healthyProviders.Any(isHealthy => isHealthy);
        }
    }
}
