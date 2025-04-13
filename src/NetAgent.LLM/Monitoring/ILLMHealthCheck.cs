namespace NetAgent.LLM.Monitoring
{
    public interface ILLMHealthCheck
    {
        Task<HealthCheckResult> CheckHealthAsync(string provider);
        Task<IDictionary<string, HealthCheckResult>> CheckAllProvidersAsync();
    }

    public class HealthCheckResult
    {
        public required HealthStatus Status { get; set; }
        public required string Message { get; set; }
        public required IDictionary<string, object> Data { get; set; }
        public DateTime CheckTime { get; set; } = DateTime.UtcNow;
    }

    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }

    public class HealthCheckOptions
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
        public bool FailureThresholdEnabled { get; set; } = true;
        public int FailureThreshold { get; set; } = 3;
        public TimeSpan FailureWindow { get; set; } = TimeSpan.FromMinutes(5);
    }
}