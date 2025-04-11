using NetAgent.LLM.AzureOpenAI;
using NetAgent.LLM.Interfaces;
using NetAgent.LLM.Ollama;
using NetAgent.LLM.OpenAI;

namespace NetAgent.LLM.Factory
{
    public static class LLMProviderFactory
    {
        public static ILLMProvider Create(LLMFactoryOptions options)
        {
            return options.Provider switch
            {
                LLMProviderType.OpenAI => new OpenAIProvider(options.OpenAI!),
                LLMProviderType.AzureOpenAI => new AzureOpenAIProvider(options.AzureOpenAI!),
                LLMProviderType.Ollama => new OllamaProvider(options.Ollama!),
                _ => throw new NotSupportedException($"Unknown LLM Provider: {options.Provider}")
            };
        }
    }
}
