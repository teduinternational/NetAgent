using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;

namespace NetAgent.LLM.Monitoring
{
    public class DefaultLLMHealthCheck : ILLMHealthCheck
    {
        private readonly IEnumerable<ILLMProvider> _providers;
        private readonly HealthCheckOptions _options;
        private readonly ConcurrentDictionary<string, List<FailureRecord>> _failureHistory;

        public DefaultLLMHealthCheck(
            IEnumerable<ILLMProvider> providers,
            IOptions<HealthCheckOptions> options)
        {
            _providers = providers;
            _options = options.Value;
            _failureHistory = new ConcurrentDictionary<string, List<FailureRecord>>();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(string provider)
        {
            var targetProvider = _providers.FirstOrDefault(p => p.Name == provider);
            if (targetProvider == null)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Message = $"Provider {provider} not found",
                    CheckTime = DateTime.UtcNow,
                    Data = new Dictionary<string, object>()
                };
            }

            try
            {
                using var cts = new CancellationTokenSource(_options.Timeout);
                var healthCheckPrompt = new Prompt { Content = "Health check test." };
                await targetProvider.GenerateAsync(healthCheckPrompt);

                // Clean up old failures and check failure threshold
                var status = GetProviderStatus(provider);

                return new HealthCheckResult
                {
                    Status = status,
                    Message = status == HealthStatus.Healthy ? 
                        "Provider is responding normally" : 
                        "Provider is experiencing intermittent failures",
                    Data = GetHealthData(provider),
                    CheckTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                RecordFailure(provider);
                return new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Message = $"Health check failed: {ex.Message}",
                    Data = GetHealthData(provider),
                    CheckTime = DateTime.UtcNow
                };
            }
        }

        public async Task<IDictionary<string, HealthCheckResult>> CheckAllProvidersAsync()
        {
            var results = new ConcurrentDictionary<string, HealthCheckResult>();
            var tasks = _providers.Select(async p =>
            {
                var result = await CheckHealthAsync(p.Name);
                results.TryAdd(p.Name, result);
            });

            await Task.WhenAll(tasks);
            return results;
        }

        private void RecordFailure(string provider)
        {
            var failures = _failureHistory.GetOrAdd(provider, _ => new List<FailureRecord>());
            lock (failures)
            {
                failures.Add(new FailureRecord { Time = DateTime.UtcNow });
                // Clean up old failures
                failures.RemoveAll(f => f.Time < DateTime.UtcNow - _options.FailureWindow);
            }
        }

        private HealthStatus GetProviderStatus(string provider)
        {
            if (!_options.FailureThresholdEnabled)
                return HealthStatus.Healthy;

            if (_failureHistory.TryGetValue(provider, out var failures))
            {
                lock (failures)
                {
                    // Clean up old failures
                    failures.RemoveAll(f => f.Time < DateTime.UtcNow - _options.FailureWindow);

                    if (failures.Count >= _options.FailureThreshold)
                        return HealthStatus.Unhealthy;
                    
                    if (failures.Count > 0)
                        return HealthStatus.Degraded;
                }
            }

            return HealthStatus.Healthy;
        }

        private IDictionary<string, object> GetHealthData(string provider)
        {
            var data = new Dictionary<string, object>();
            
            if (_failureHistory.TryGetValue(provider, out var failures))
            {
                lock (failures)
                {
                    failures.RemoveAll(f => f.Time < DateTime.UtcNow - _options.FailureWindow);
                    data["recent_failures"] = failures.Count;
                    data["last_failure"] = failures.Any() ? 
                        failures.Max(f => f.Time).ToString("o") : null;
                }
            }

            return data;
        }

        private class FailureRecord
        {
            public DateTime Time { get; set; }
        }
    }
}