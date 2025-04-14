using System;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.DeepSeek
{
    public class DeepSeekLLMProviderPlugin : ILLMProviderPlugin
    {
        public string Name => "DeepSeek";

        public Type ConfigurationType => typeof(DeepSeekOptions);

        public ILLMProvider CreateProvider(IServiceProvider services)
        {
            var options = services.GetRequiredService<DeepSeekOptions>();
            return new DeepSeekLLMProvider(options);
        }
    }
}