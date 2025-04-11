using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Strategy
{
    public class AgentDecision
    {
        public string Plan { get; set; } = string.Empty;
        public string? ToolToUse { get; set; }
        public bool SkipToolExecution { get; set; } = false;
    }
}
