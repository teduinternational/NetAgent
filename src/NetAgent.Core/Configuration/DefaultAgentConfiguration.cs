using System.Collections.Concurrent;
using NetAgent.Abstractions;
using NetAgent.Abstractions.Models;

namespace NetAgent.Core.Configuration
{
    public class DefaultAgentConfiguration : IAgentConfiguration
    {
        private readonly ConcurrentDictionary<string, object> _settings = new();
        private AgentOptions _currentOptions;

        public DefaultAgentConfiguration()
        {
            _currentOptions = GetDefaultOptions();
        }

        public AgentOptions GetDefaultOptions()
        {
            return new AgentOptions
            {
                MaxPlanSteps = 10,
                TimeoutSeconds = 30,
                VerboseLogging = false,
                ProviderSettings = new Dictionary<string, object>(),
                Memory = new MemoryOptions(),
                Tools = new ToolOptions
                {
                    EnabledTools = new List<string>(),
                    ToolSettings = new Dictionary<string, object>()
                }
            };
        }

        public void ConfigureAgent(AgentOptions options)
        {
            _currentOptions = options;
            
            // Update settings dictionary
            foreach (var setting in options.ProviderSettings)
            {
                SetValue(setting.Key, setting.Value);
            }

            foreach (var setting in options.Tools.ToolSettings)
            {
                SetValue($"tool:{setting.Key}", setting.Value);
            }
        }

        public IEnumerable<string> ValidateConfiguration()
        {
            var errors = new List<string>();

            if (_currentOptions.MaxPlanSteps <= 0)
            {
                errors.Add("MaxPlanSteps must be greater than 0");
            }

            if (_currentOptions.TimeoutSeconds <= 0)
            {
                errors.Add("TimeoutSeconds must be greater than 0");
            }

            if (_currentOptions.Memory.ContextWindowSize <= 0)
            {
                errors.Add("ContextWindowSize must be greater than 0");
            }

            return errors;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (_settings.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
            _settings.AddOrUpdate(key, value, (_, _) => value);
        }
    }
}