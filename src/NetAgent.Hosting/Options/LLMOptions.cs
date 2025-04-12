using NetAgent.LLM.OpenAI;
using NetAgent.LLM.Claude;
using NetAgent.LLM.DeepSeek;
using NetAgent.LLM.Gemini;

namespace NetAgent.Hosting.Options
{
    public class LLMOptions
    {
        public string? Provider { get; set; }
        public string[]? Providers { get; set; }
        public OpenAIOptions? OpenAI { get; set; }
        public ClaudeLLMOptions? Claude { get; set; }
        public DeepSeekOptions? DeepSeek { get; set; }
        public GeminiOptions? Gemini { get; set; }
    }
}
