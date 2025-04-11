using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Models
{
    /// <summary>
    /// Định nghĩa cấu hình cho agent
    /// </summary>
    public class AgentOptions
    {
        public string DefaultLLMProvider { get; set; } = "OpenAI";
        public List<string> EnabledTools { get; set; } = new();
    }
}
