using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NetAgent.Abstractions.LLM;

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
            ILLMPreferences preferences = null)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
            _scorer = scorer ?? throw new ArgumentNullException(nameof(scorer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _preferences = preferences;
        }

        public string Name => "MultiLLM";

        public async Task<string[]> GenerateFromAllAsync(string prompt)
        {
            var results = new List<string>();
            var orderedProviders = OrderProvidersByPreference(_providers);

            foreach (var provider in orderedProviders)
            {
                if (!IsProviderAvailable(provider))
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

        public async Task<string> GenerateBestAsync(string prompt)
        {
            var results = await GenerateFromAllAsync(prompt);
            return results
                .OrderByDescending(r => _scorer.ScoreResponse(r))
                .FirstOrDefault() ?? string.Empty;
        }

        public async Task<string> GenerateAsync(string prompt, string goal = "", string context = "")
        {
            var orderedProviders = OrderProvidersByPreference(_providers);

            foreach (var provider in orderedProviders)
            {
                if (!IsProviderAvailable(provider))
                    continue;

                try
                {
                    var result = await provider.GenerateAsync(prompt, goal, context);
                    _failedProviders.TryRemove(provider.Name, out _);
                    return result;
                }
                catch (LLMException ex)
                {
                    _logger.LogWarning("Provider {Provider} failed: {Message}", provider.Name, ex.Message);
                    HandleProviderFailure(provider.Name);
                }
            }

            throw new LLMException("All available LLM providers failed or are temporarily disabled");
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
                return false;

            return !IsProviderTemporarilyDisabled(provider.Name);
        }

        private bool IsProviderTemporarilyDisabled(string providerName)
        {
            if (_failedProviders.TryGetValue(providerName, out var failedTime))
            {
                if (DateTime.UtcNow - failedTime < TimeSpan.FromMinutes(RETRY_MINUTES))
                    return true;
                
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
        }
    }
}
