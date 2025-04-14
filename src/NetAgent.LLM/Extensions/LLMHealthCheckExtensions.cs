using Microsoft.Extensions.DependencyInjection;
using NetAgent.LLM.Monitoring;

namespace NetAgent.LLM.Extensions
{
    public static class LLMHealthCheckExtensions
    {
        public static IServiceCollection AddLLMHealthChecks(
            this IServiceCollection services,
            Action<HealthCheckOptions> configureOptions = null)
        {
            // Configure health check options
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            // Register health check service
            services.AddSingleton<ILLMHealthCheck, DefaultLLMHealthCheck>();

            return services;
        }
        public static IServiceCollection AddLLMMonitoringSystem(
            this IServiceCollection services,
            Action<HealthCheckOptions> configureHealthChecks = null,
            Action<LLMMetricsOptions> configureMetrics = null)
        {
            // Add health checks
            services.AddLLMHealthChecks(configureHealthChecks);

            // Add metrics monitoring
            services.AddLLMMonitoring(configureMetrics);

            return services;
        }
    }
}