using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Core.Planning
{
    public class Plan
    {
        public string Goal { get; set; } = string.Empty;
        public List<PlanStep> Steps { get; set; } = new();
    }
}
