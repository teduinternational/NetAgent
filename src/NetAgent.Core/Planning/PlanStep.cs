using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Core.Planning
{
    public class PlanStep
    {
        public string Action { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
    }
}
