using System;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.Gemini
{
    public class GeminiLLMProviderPlugin : ILLMProviderPlugin
    {
        public string Name => "Gemini";

        public Type ConfigurationType => typeof(GeminiOptions);

        public ILLMProvider CreateProvider(IServiceProvider services)
        {
            var options = services.GetRequiredService<GeminiOptions>();
            return new GeminiLLMProvider(options);
        }
    }
}