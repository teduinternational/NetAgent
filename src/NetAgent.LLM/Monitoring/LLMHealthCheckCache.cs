using System.Collections.Concurrent;

namespace NetAgent.LLM.Monitoring
{
    public class LLMHealthCheckCache
    {
        private static readonly ConcurrentDictionary<string, HealthCheckResult> _cache = new();
        private static readonly ConcurrentDictionary<string, DateTime> _lastCheckTime = new();
        private static readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(1);

        public static bool TryGetCachedResults(out IDictionary<string, HealthCheckResult> results)
        {
            results = null;
            if (!_cache.Any()) return false;

            // Check if cache is still valid
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

        public static void UpdateCache(IDictionary<string, HealthCheckResult> results)
        {
            var now = DateTime.UtcNow;
            _cache.Clear();
            _lastCheckTime.Clear();

            foreach (var (provider, result) in results)
            {
                _cache[provider] = result;
                _lastCheckTime[provider] = now;
            }
        }
    }
}