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
        public Dictionary<string, double> DetailedScores { get; set; } = new();
        public string Feedback { get; set; } = string.Empty;
    }
}
