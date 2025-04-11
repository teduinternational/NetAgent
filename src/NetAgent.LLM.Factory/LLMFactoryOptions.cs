using NetAgent.LLM.AzureOpenAI;
using NetAgent.LLM.Ollama;
using NetAgent.LLM.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.LLM.Factory
{
    public class LLMFactoryOptions
    {
        public LLMProviderType Provider { get; set; }

        public OpenAIOptions? OpenAI { get; set; }
        public AzureOpenAIOptions? AzureOpenAI { get; set; }
        public OllamaOptions? Ollama { get; set; }
    }

}
