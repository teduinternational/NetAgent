using NetAgent.Abstractions.LLM;
using NetAgent.LLM.OpenAI;
using NetAgent.LLM.DeepSeek;
using NetAgent.LLM.Gemini;
using NetAgent.LLM.Claude;

namespace NetAgent.LLM.Factory
{
    public static class LLMProviderFactory
    {
        public static ILLMProvider Create(LLMFactoryOptions options)
        {
            return options.Provider switch
            {
                LLMProviderType.OpenAI => new OpenAIProvider(options.OpenAI!),
                LLMProviderType.Claude => new ClaudeLLMProvider(options.Claude!),
                LLMProviderType.DeepSeek => new DeepSeekLLMProvider(options.DeepSeek!),
                LLMProviderType.Gemini => new GeminiLLMProvider(options.Gemini!),
                _ => throw new NotSupportedException($"Unknown LLM Provider: {options.Provider}")
            };
        }
    }
}
