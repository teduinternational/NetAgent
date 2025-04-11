using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Models
{
    public class AgentResult
    {
        public string Output { get; set; } = string.Empty;
        public string FinalPrompt { get; set; } = string.Empty;
    }

}
