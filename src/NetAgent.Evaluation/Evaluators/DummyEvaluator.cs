using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Evaluation.Evaluators
{
    public class DummyEvaluator : IEvaluator
    {
        public Task<EvaluationResult> EvaluateAsync(string prompt, string output, string goal, string context)
        {
            return Task.FromResult(new EvaluationResult
            {
                Score = 1.0,
                Summary = "Dummy always returns perfect score."
            });
        }
    }
}
