using System;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.OpenAI
{
    public class OpenAIProviderPlugin : ILLMProviderPlugin
    {
        public string Name => "OpenAI";

        public Type ConfigurationType => typeof(OpenAIOptions);

        public ILLMProvider CreateProvider(IServiceProvider services)
        {
            var options = services.GetRequiredService<OpenAIOptions>();
            return new OpenAIProvider(options);
        }
    }
}