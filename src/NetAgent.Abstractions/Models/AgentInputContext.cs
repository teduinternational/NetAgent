using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Models
{
    public class AgentInputContext
    {
        public string Goal { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
