using System;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.Claude
{
    public class ClaudeLLMProviderPlugin : ILLMProviderPlugin
    {
        public string Name => "Claude";

        public Type ConfigurationType => typeof(ClaudeLLMOptions);

        public ILLMProvider CreateProvider(IServiceProvider services)
        {
            var options = services.GetRequiredService<ClaudeLLMOptions>();
            return new ClaudeLLMProvider(options);
        }
    }
}