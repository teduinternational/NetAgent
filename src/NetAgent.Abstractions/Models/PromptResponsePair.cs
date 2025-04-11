using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Models
{
    public class PromptResponsePair
    {
        public Prompt Prompt { get; set; }
        public LLMResponse Response { get; set; }
    }
}
