using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Hosting.Options
{
    public class NetAgentOptions
    {
        public LLMOptions? LLM { get; set; }
        public PlannerOptions? Planner { get; set; }
        public MemoryOptions? Memory { get; set; }
        public string[] Tools { get; set; } = Array.Empty<string>();
        public ContextSourceOptions? Context { get; set; }
    }
}
