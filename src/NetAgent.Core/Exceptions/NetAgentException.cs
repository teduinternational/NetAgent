using System;

namespace NetAgent.Core.Exceptions
{
    public class NetAgentException : Exception
    {
        public NetAgentException(string message) : base(message) { }
        public NetAgentException(string message, Exception inner) : base(message, inner) { }
    }

    public class ConfigurationException : NetAgentException
    {
        public ConfigurationException(string message) : base(message) { }
        public ConfigurationException(string message, Exception inner) : base(message, inner) { }
    }

    public class ProviderException : NetAgentException 
    {
        public ProviderException(string message) : base(message) { }
        public ProviderException(string message, Exception inner) : base(message, inner) { }
    }

    public class ToolException : NetAgentException
    {
        public ToolException(string message) : base(message) { }
        public ToolException(string message, Exception inner) : base(message, inner) { }
    }
}