using Microsoft.Extensions.Options;
using NetAgent.Abstractions.LLM;
using System.Collections.Concurrent;

namespace NetAgent.LLM.Monitoring
{
    public class DefaultLLMHealthCheck : ILLMHealthCheck
    {
        private readonly IEnumerable<ILLMProvider> _providers;
        private readonly HealthCheckOptions _options;
        private readonly ConcurrentDictionary<string, Queue<(DateTime time, bool success)>> _healthHistory;
        private static readonly ConcurrentDictionary<string, HealthCheckResult> _cache = new();
        private static readonly ConcurrentDictionary<string, DateTime> _lastCheckTime = new();
        private static readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(1);

        public DefaultLLMHealthCheck(IEnumerable<ILLMProvider> providers, IOptions<HealthCheckOptions> options)
        {
            _providers = providers;
            _options = options.Value;
            _healthHistory = new ConcurrentDictionary<string, Queue<(DateTime, bool)>>();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(string provider)
        {
            // Check cache first
            if (TryGetCachedResult(provider, out var cachedResult))
            {
                return cachedResult;
            }

            var llmProvider = _providers.FirstOrDefault(p => p.Name == provider);
            if (llmProvider == null)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Message = $"Provider {provider} not found",
                    Data = new Dictionary<string, object>()
                };
            }

            try
            {
                using var cts = new CancellationTokenSource(_options.Timeout);

                // Send a simple test prompt
                var response = await llmProvider.GenerateAsync(new Abstractions.Models.Prompt
                {
                    Content = "Test health check."
                });

                var success = !string.IsNullOrEmpty(response?.Content);
                UpdateHealthHistory(provider, success);

                var status = DetermineHealthStatus(provider);
                var result = new HealthCheckResult
                {
                    Status = status,
                    Message = GetStatusMessage(status),
                    Data = GetHealthData(provider)
                };

                // Cache the result
                UpdateCache(provider, result);

                return result;
            }
            catch (Exception ex)
            {
                UpdateHealthHistory(provider, false);
                var result = new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Message = $"Health check failed: {ex.Message}",
                    Data = GetHealthData(provider)
                };

                // Cache the failure result
                UpdateCache(provider, result);

                return result;
            }
        }

        public async Task<IDictionary<string, HealthCheckResult>> CheckAllProvidersAsync()
        {
            // Try to get all results from cache first
            if (TryGetAllCachedResults(out var cachedResults))
            {
                return cachedResults;
            }

            // Group providers by name and select the first one in each group to avoid duplicates
            var uniqueProviders = _providers.GroupBy(p => p.Name)
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .Select(g => g.First());

            var tasks = uniqueProviders.Select(async provider =>
            {
                var result = await CheckHealthAsync(provider.Name);
                return new KeyValuePair<string, HealthCheckResult>(provider.Name, result);
            });

            var results = await Task.WhenAll(tasks);
            return new Dictionary<string, HealthCheckResult>(results);
        }

        private void UpdateHealthHistory(string provider, bool success)
        {
            var history = _healthHistory.GetOrAdd(provider, _ => new Queue<(DateTime, bool)>());

            // Add new result
            history.Enqueue((DateTime.UtcNow, success));

            // Remove old entries outside the failure window
            while (history.Count > 0 &&
                   DateTime.UtcNow - history.Peek().time > _options.FailureWindow)
            {
                history.Dequeue();
            }
        }

        private HealthStatus DetermineHealthStatus(string provider)
        {
            if (!_healthHistory.TryGetValue(provider, out var history))
                return HealthStatus.Unhealthy;

            if (!_options.FailureThresholdEnabled)
                return history.Any() && history.Last().success
                    ? HealthStatus.Healthy
                    : HealthStatus.Unhealthy;

            var recentFailures = history.Count(h => !h.success);
            if (recentFailures >= _options.FailureThreshold)
                return HealthStatus.Unhealthy;
            if (recentFailures > 0)
                return HealthStatus.Degraded;

            return HealthStatus.Healthy;
        }

        private string GetStatusMessage(HealthStatus status) => status switch
        {
            HealthStatus.Healthy => "Provider is healthy",
            HealthStatus.Degraded => "Provider is experiencing intermittent failures",
            HealthStatus.Unhealthy => "Provider is unhealthy",
            _ => "Unknown health status"
        };

        private Dictionary<string, object> GetHealthData(string provider)
        {
            if (!_healthHistory.TryGetValue(provider, out var history))
                return new Dictionary<string, object>();

            var data = new Dictionary<string, object>
            {
                ["totalChecks"] = history.Count,
                ["failureCount"] = history.Count(h => !h.success),
                ["lastCheck"] = history.LastOrDefault().time,
                ["failureThreshold"] = _options.FailureThreshold,
                ["failureWindow"] = _options.FailureWindow.TotalSeconds
            };

            return data;
        }

        private bool TryGetCachedResult(string provider, out HealthCheckResult result)
        {
            result = null;
            if (!_cache.TryGetValue(provider, out result)) return false;
            if (!_lastCheckTime.TryGetValue(provider, out var lastCheck)) return false;

            if (DateTime.UtcNow - lastCheck <= _cacheExpiration)
            {
                return true;
            }

            // Remove expired cache entry
            _cache.TryRemove(provider, out _);
            _lastCheckTime.TryRemove(provider, out _);
            return false;
        }

        private bool TryGetAllCachedResults(out IDictionary<string, HealthCheckResult> results)
        {
            results = null;
            if (!_cache.Any()) return false;

            var now = DateTime.UtcNow;
            if (_lastCheckTime.Values.All(t => (now - t) <= _cacheExpiration))
            {
                results = new Dictionary<string, HealthCheckResult>(_cache);
                return true;
            }

            // Clear expired cache
            _cache.Clear();
            _lastCheckTime.Clear();
            return false;
        }

        private void UpdateCache(string provider, HealthCheckResult result)
        {
            _cache[provider] = result;
            _lastCheckTime[provider] = DateTime.UtcNow;
        }
    }
}