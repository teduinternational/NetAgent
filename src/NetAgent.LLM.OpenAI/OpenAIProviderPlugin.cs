using System;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.OpenAI
{
    public class OpenAIProviderPlugin : ILLMProviderPlugin
    {
        public string Name => "openai";
        
        public Type ConfigurationType => typeof(OpenAIOptions);
        
        public ILLMProvider CreateProvider(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<OpenAIOptions>();
            return new OpenAIProvider(options);
        }
    }
}