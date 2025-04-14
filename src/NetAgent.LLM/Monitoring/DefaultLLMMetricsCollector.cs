using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using NetAgent.Abstractions.Models;

namespace NetAgent.LLM.Monitoring
{
    public class DefaultLLMMetricsCollector : ILLMMetricsCollector
    {
        private readonly Meter _meter;
        private readonly LLMMetricsOptions _options;
        private readonly Counter<long> _errorCounter;
        private readonly Counter<long> _responseCounter;
        private readonly Histogram<double> _latencyHistogram;
        private readonly Counter<long> _tokenCounter;
        private readonly Counter<long> _healthCounter;

        public DefaultLLMMetricsCollector(IOptions<LLMMetricsOptions> options)
        {
            _options = options.Value;
            _meter = new Meter(MetricsConstants.MeterName);

            // Always initialize the metrics but only record if enabled
            _errorCounter = _meter.CreateCounter<long>(
                MetricsConstants.ErrorsMetricName,
                description: "Number of LLM errors");

            _responseCounter = _meter.CreateCounter<long>(
                MetricsConstants.ResponsesMetricName,
                description: "Number of LLM responses");

            _latencyHistogram = _meter.CreateHistogram<double>(
                MetricsConstants.LatencyMetricName,
                unit: "ms",
                description: "LLM request duration");

            _tokenCounter = _meter.CreateCounter<long>(
                MetricsConstants.TokensMetricName,
                description: "Number of tokens used");

            _healthCounter = _meter.CreateCounter<long>(
                MetricsConstants.HealthMetricName,
                description: "Health status of LLM providers");
        }

        public void RecordLatency(string provider, double milliseconds)
        {
            if (_options.EnableLatencyTracking)
            {
                var tag = new KeyValuePair<string, object?>("provider", provider);
                _latencyHistogram.Record(milliseconds, tag);
            }
        }

        public void RecordTokens(string provider, int tokens)
        {
            if (_options.EnableTokenCounting)
            {
                var tag = new KeyValuePair<string, object?>("provider", provider);
                _tokenCounter.Add(tokens, tag);
            }
        }

        public void RecordError(string provider, string errorType)
        {
            if (_options.EnableErrorTracking)
            {
                var tags = new[]
                {
                    new KeyValuePair<string, object?>("provider", provider),
                    new KeyValuePair<string, object?>("error_type", errorType)
                };
                _errorCounter.Add(1, tags);
            }
        }

        public void RecordResponse(string provider, LLMResponse response)
        {
            if (_options.EnableResponseTracking)
            {
                var tag = new KeyValuePair<string, object?>("provider", provider);
                _responseCounter.Add(1, tag);
            }
        }

        public void RecordHealth(string provider, bool isHealthy)
        {
            if (_options.EnableHealthTracking)
            {
                var tags = new[]
                {
                    new KeyValuePair<string, object?>("provider", provider),
                    new KeyValuePair<string, object?>("status", isHealthy ? "healthy" : "unhealthy")
                };
                _healthCounter.Add(1, tags);
            }
        }
    }
}