using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Optimization.Models
{
    public class OptimizationResult
    {
        public string OptimizedPrompt { get; set; } = string.Empty;
        public string[] Suggestions { get; set; } = Array.Empty<string>();
    }
}
