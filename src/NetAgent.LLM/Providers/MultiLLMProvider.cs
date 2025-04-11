using NetAgent.Abstractions.LLM;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace NetAgent.LLM.Providers
{
    public class MultiLLMProvider : IMultiLLMProvider 
    {
        private readonly IEnumerable<ILLMProvider> _providers;
        private readonly IResponseScorer _scorer;
        private readonly ILogger<MultiLLMProvider> _logger;
        private readonly ConcurrentDictionary<string, DateTime> _failedProviders = new();
        private const int RETRY_MINUTES = 5;

        public MultiLLMProvider(
            IEnumerable<ILLMProvider> providers, 
            IResponseScorer scorer,
            ILogger<MultiLLMProvider> logger)
        {
            _providers = providers;
            _scorer = scorer;
            _logger = logger;
        }

        public string Name => "MultiLLM";

        public async Task<string[]> GenerateFromAllAsync(string prompt)
        {
            var results = new List<string>();
            foreach (var provider in _providers)
            {
                if (IsProviderTemporarilyDisabled(provider.Name))
                    continue;

                try
                {
                    var result = await provider.GenerateAsync(prompt);
                    results.Add(result);
                    // If previously failed provider succeeds, remove it from failed list
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
            foreach (var provider in _providers)
            {
                if (IsProviderTemporarilyDisabled(provider.Name))
                    continue;

                try
                {
                    var result = await provider.GenerateAsync(prompt, goal, context);
                    // If previously failed provider succeeds, remove it from failed list
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

        private bool IsProviderTemporarilyDisabled(string providerName)
        {
            if (_failedProviders.TryGetValue(providerName, out var failedTime))
            {
                if (DateTime.UtcNow - failedTime < TimeSpan.FromMinutes(RETRY_MINUTES))
                    return true;
                
                // Retry time elapsed, remove from failed list
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
