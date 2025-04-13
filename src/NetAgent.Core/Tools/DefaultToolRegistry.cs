using System.Collections.Concurrent;
using System.Collections.Generic;
using NetAgent.Abstractions.Tools;

namespace NetAgent.Core.Tools
{
    public class DefaultToolRegistry : IToolRegistry
    {
        private readonly ConcurrentDictionary<string, IAgentTool> _tools = new();

        public void RegisterTool(string name, IAgentTool tool)
        {
            if (!_tools.TryAdd(name, tool))
            {
                throw new ToolRegistrationException($"Tool with name {name} is already registered");
            }
        }

        public IAgentTool GetTool(string name)
        {
            if (_tools.TryGetValue(name, out var tool))
            {
                return tool;
            }
            throw new ToolNotFoundException($"Tool {name} not found");
        }

        public IEnumerable<IAgentTool> GetAllTools()
        {
            return _tools.Values;
        }

        public bool HasTool(string name)
        {
            return _tools.ContainsKey(name);
        }

        public bool RemoveTool(string name)
        {
            return _tools.TryRemove(name, out _);
        }
    }

    public class ToolRegistrationException : System.Exception
    {
        public ToolRegistrationException(string message) : base(message) { }
    }

    public class ToolNotFoundException : System.Exception
    {
        public ToolNotFoundException(string message) : base(message) { }
    }
}