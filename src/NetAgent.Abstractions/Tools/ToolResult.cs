using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Tools
{
    public class ToolResult
    {
        public string ToolName { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
