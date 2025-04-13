using NetAgent.Optimization.Interfaces;
using NetAgent.Optimization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Optimization.Optimizers
{
    public class DummyOptimizer : IOptimizer
    {
        public Task<bool> IsHealthyAsync()
        {
            return Task.FromResult(true);
        }

        public Task<OptimizationResult> OptimizeAsync(string prompt, string goal, string context)
        {
            return Task.FromResult(new OptimizationResult
            {
                OptimizedPrompt = prompt,
                Suggestions = new[] { "No optimization applied (dummy)" }
            });
        }
    }
}
