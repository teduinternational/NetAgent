using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Models
{
    public class AgentContext
    {
        public string Goal { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string ToolOutput { get; set; } = string.Empty;
    }
}
