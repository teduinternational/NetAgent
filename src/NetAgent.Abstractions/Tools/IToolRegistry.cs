using System.Collections.Generic;

namespace NetAgent.Abstractions.Tools
{
    public interface IToolRegistry
    {
        /// <summary>
        /// Registers a tool with the specified name
        /// </summary>
        void RegisterTool(string name, IAgentTool tool);

        /// <summary>
        /// Gets a tool by name
        /// </summary>
        IAgentTool GetTool(string name);

        /// <summary>
        /// Gets all registered tools
        /// </summary>
        IEnumerable<IAgentTool> GetAllTools();

        /// <summary>
        /// Checks if a tool with the specified name exists
        /// </summary>
        bool HasTool(string name);

        /// <summary>
        /// Removes a tool from the registry
        /// </summary>
        bool RemoveTool(string name);
    }
}