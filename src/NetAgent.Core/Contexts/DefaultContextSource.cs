using NetAgent.Abstractions.Models;
using System.Diagnostics;

namespace NetAgent.Core.Contexts
{
    public class DefaultContextSource : IContextSource
    {
        private readonly string _environment;
        private readonly string _version;
        private readonly Dictionary<string, object> _systemConfig;

        public DefaultContextSource(
            string environment = "Development",
            string version = "1.0.0",
            Dictionary<string, object>? systemConfig = null)
        {
            _environment = environment;
            _version = version;
            _systemConfig = systemConfig ?? new Dictionary<string, object>();
        }

        public async Task<AgentInputContext> GetContextAsync()
        {
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            var context = new AgentInputContext
            {
                Context = "Default agent context",
                Metadata = new Dictionary<string, string>
                {
                    // Environment info
                    { "Env", _environment },
                    { "Version", _version },
                    { "Timezone", TimeZoneInfo.Local.Id },
                    { "Timestamp", DateTime.UtcNow.ToString("o") },
                    
                    // Session info
                    { "SessionId", Guid.NewGuid().ToString() },
                    { "TraceId", traceId },
                    { "CorrelationId", traceId },
                    
                    // System info
                    { "OS", Environment.OSVersion.ToString() },
                    { "ProcessorCount", Environment.ProcessorCount.ToString() },
                    { "MachineName", Environment.MachineName },
                    
                    // Agent info
                    { "AgentId", Guid.NewGuid().ToString() },
                    { "MaxMemoryMB", "1024" },
                    { "TimeoutSeconds", "300" }
                }
            };

            // Add business context
            context.Metadata["ProjectId"] = GetCurrentProjectId();
            context.Metadata["OrganizationId"] = GetCurrentOrganizationId();

            // Add system configuration
            foreach (var config in _systemConfig)
            {
                context.Metadata[$"Config_{config.Key}"] = config.Value?.ToString() ?? "";
            }

            // Add performance metrics
            AddPerformanceMetrics(context.Metadata);

            return context;
        }

        private void AddPerformanceMetrics(Dictionary<string, string> metadata)
        {
            var process = Process.GetCurrentProcess();
            metadata["ProcessStartTime"] = process.StartTime.ToString("o");
            metadata["TotalProcessorTime"] = process.TotalProcessorTime.ToString();
            metadata["WorkingSet64"] = process.WorkingSet64.ToString();
        }

        private string GetCurrentProjectId() => 
            // Implementation based on your project context
            Environment.GetEnvironmentVariable("PROJECT_ID") ?? "default-project";

        private string GetCurrentOrganizationId() => 
            // Implementation based on your organization context
            Environment.GetEnvironmentVariable("ORGANIZATION_ID") ?? "default-org";
    }
}
