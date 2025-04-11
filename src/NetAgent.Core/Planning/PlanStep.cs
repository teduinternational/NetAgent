using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Core.Planning
{
    public class PlanStep
    {
        public string Description { get; set; } = string.Empty; // Mô tả bước
        public string ToolToUse { get; set; } = string.Empty;   // Tool nếu có
        public string Input { get; set; } = string.Empty;       // Input cho tool
        public bool IsFinalStep { get; set; }                   // Bước cuối?
    }
}
