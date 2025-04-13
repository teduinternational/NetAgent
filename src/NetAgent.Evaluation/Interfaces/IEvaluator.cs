using NetAgent.Evaluation.Models;
using NetAgent.LLM.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Evaluation.Interfaces
{
    public interface IEvaluator
    {
        Task<bool> IsHealthyAsync();
        Task<EvaluationResult> EvaluateAsync(string prompt, string response, string goal, string context);
    }
}
