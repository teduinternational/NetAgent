using System;

namespace NetAgent.Abstractions.LLM
{
    public class LLMException : Exception
    {
        public LLMException(string message) : base(message)
        {
        }

        public LLMException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
