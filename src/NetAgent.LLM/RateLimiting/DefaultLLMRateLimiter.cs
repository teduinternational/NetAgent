using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace NetAgent.LLM.RateLimiting
{
    public class DefaultLLMRateLimiter : ILLMRateLimiter
    {
        private readonly RateLimitOptions _options;
        private readonly ConcurrentDictionary<string, ProviderState> _states = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

        public DefaultLLMRateLimiter(IOptions<RateLimitOptions> options)
        {
            _options = options.Value;
        }

        public async Task WaitAsync(string provider)
        {
            var state = _states.GetOrAdd(provider, _ => new ProviderState());
            var semaphore = _semaphores.GetOrAdd(provider, _ => new SemaphoreSlim(_options.ConcurrentRequests));

            // Check and update request rate
            await CheckRequestRateAsync(provider, state);

            // Wait for available concurrent slot
            await semaphore.WaitAsync();

            try
            {
                // Final check before proceeding
                await CheckRequestRateAsync(provider, state);
            }
            catch
            {
                semaphore.Release();
                throw;
            }
        }

        public void ReportSuccess(string provider)
        {
            if (_semaphores.TryGetValue(provider, out var semaphore))
            {
                semaphore.Release();
            }

            if (_states.TryGetValue(provider, out var state))
            {
                lock (state)
                {
                    state.ConsecutiveFailures = 0;
                    if (_options.EnableAdaptiveThrottling)
                    {
                        state.CurrentLimit = Math.Min(
                            state.CurrentLimit + 1,
                            _options.RequestsPerMinute
                        );
                    }
                }
            }
        }

        public void ReportFailure(string provider)
        {
            if (_semaphores.TryGetValue(provider, out var semaphore))
            {
                semaphore.Release();
            }

            if (_states.TryGetValue(provider, out var state))
            {
                lock (state)
                {
                    state.ConsecutiveFailures++;
                    if (_options.EnableAdaptiveThrottling)
                    {
                        state.CurrentLimit = Math.Max(
                            1,
                            state.CurrentLimit - state.ConsecutiveFailures
                        );
                    }
                }
            }
        }

        private async Task CheckRequestRateAsync(string provider, ProviderState state)
        {
            var now = DateTime.UtcNow;
            
            lock (state)
            {
                // Clean up old requests
                while (state.RequestTimestamps.Count > 0 && 
                       now - state.RequestTimestamps.Peek() > TimeSpan.FromMinutes(1))
                {
                    state.RequestTimestamps.Dequeue();
                }

                var currentLimit = _options.EnableAdaptiveThrottling ? 
                    state.CurrentLimit : 
                    _options.RequestsPerMinute;

                if (state.RequestTimestamps.Count >= currentLimit)
                {
                    var oldestRequest = state.RequestTimestamps.Peek();
                    var waitTime = oldestRequest.AddMinutes(1) - now;
                    throw new RateLimitExceededException(provider, waitTime);
                }

                state.RequestTimestamps.Enqueue(now);
            }

            await Task.CompletedTask;
        }

        private class ProviderState
        {
            public Queue<DateTime> RequestTimestamps { get; } = new Queue<DateTime>();
            public int ConsecutiveFailures { get; set; }
            public int CurrentLimit { get; set; }

            public ProviderState()
            {
                CurrentLimit = 60; // Start with default limit
            }
        }
    }
}