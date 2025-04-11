using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Evaluation.Models
{
    public class EvaluationResult
    {
        public double Score { get; set; }
        public string[] Weaknesses { get; set; } = Array.Empty<string>();
        public string[] Suggestions { get; set; } = Array.Empty<string>();
        public string Summary { get; set; } = string.Empty;
    }
}
