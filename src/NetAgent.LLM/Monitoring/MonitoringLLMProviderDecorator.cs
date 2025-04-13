using System.Diagnostics;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;

namespace NetAgent.LLM.Monitoring
{
    public class MonitoringLLMProviderDecorator : ILLMProvider 
    {
        private readonly ILLMProvider _innerProvider;
        private readonly ILLMMetricsCollector _metrics;

        public MonitoringLLMProviderDecorator(
            ILLMProvider provider,
            ILLMMetricsCollector metrics)
        {
            _innerProvider = provider;
            _metrics = metrics;
        }

        public string Name => _innerProvider.Name;

        public async Task<LLMResponse> GenerateAsync(Prompt prompt)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await _innerProvider.GenerateAsync(prompt);
                stopwatch.Stop();

                // Record metrics
                _metrics.RecordLatency(Name, stopwatch.ElapsedMilliseconds);
                _metrics.RecordResponse(Name, response);
                
                // Estimate tokens if available
                if (response.Metadata?.ContainsKey("total_tokens") == true &&
                    int.TryParse(response.Metadata["total_tokens"].ToString(), out var tokens))
                {
                    _metrics.RecordTokens(Name, tokens);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _metrics.RecordLatency(Name, stopwatch.ElapsedMilliseconds);
                _metrics.RecordError(Name, ex.GetType().Name);
                throw;
            }
        }
    }
}