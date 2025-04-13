using System.Collections.Generic;
using NetAgent.Abstractions.Models;

namespace NetAgent.Abstractions
{
    public interface IAgentConfiguration
    {
        /// <summary>
        /// Gets the default agent options
        /// </summary>
        AgentOptions GetDefaultOptions();

        /// <summary>
        /// Configures the agent with the specified options
        /// </summary>
        void ConfigureAgent(AgentOptions options);

        /// <summary>
        /// Validates the current configuration
        /// </summary>
        /// <returns>A list of validation errors, empty if valid</returns>
        IEnumerable<string> ValidateConfiguration();

        /// <summary>
        /// Gets the current configuration value
        /// </summary>
        T GetValue<T>(string key, T defaultValue = default);

        /// <summary>
        /// Sets a configuration value
        /// </summary>
        void SetValue<T>(string key, T value);
    }
}