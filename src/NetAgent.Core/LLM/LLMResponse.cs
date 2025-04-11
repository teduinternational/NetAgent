using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Core.LLM
{
    public class LLMResponse
    {
        public string Result { get; set; } = string.Empty;
        public double? Cost { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}
