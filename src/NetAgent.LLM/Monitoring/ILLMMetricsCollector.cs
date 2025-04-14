using System.Diagnostics.Metrics;
using NetAgent.Abstractions.Models;

namespace NetAgent.LLM.Monitoring
{
    public interface ILLMMetricsCollector
    {
        void RecordLatency(string provider, double milliseconds);
        void RecordTokens(string provider, int tokens);
        void RecordError(string provider, string errorType);
        void RecordResponse(string provider, LLMResponse response);
        void RecordHealth(string provider, bool isHealthy);
    }

    public class LLMMetricsOptions
    {
        public bool EnableLatencyTracking { get; set; } = true;
        public bool EnableTokenCounting { get; set; } = true;
        public bool EnableErrorTracking { get; set; } = true;
        public bool EnableResponseTracking { get; set; } = true;
        public bool EnableHealthTracking { get; set; } = true;
    }

    public class MetricsConstants
    {
        public const string MeterName = "NetAgent.LLM";
        public const string LatencyMetricName = "llm_request_duration";
        public const string TokensMetricName = "llm_tokens_used";
        public const string ErrorsMetricName = "llm_errors_total";
        public const string ResponsesMetricName = "llm_responses_total";
        public const string HealthMetricName = "llm_health_status";
    }
}