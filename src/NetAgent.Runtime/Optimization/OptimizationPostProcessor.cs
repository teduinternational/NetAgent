using NetAgent.Abstractions.Models;
using NetAgent.Optimization.Interfaces;
using NetAgent.Runtime.PostProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Runtime.Optimization
{
    public class OptimizationPostProcessor : IAgentPostProcessor
    {
        private readonly IOptimizer _optimizer;

        public OptimizationPostProcessor(IOptimizer optimizer)
        {
            _optimizer = optimizer;
        }

        public async Task PostProcessAsync(AgentResponse result, AgentInputContext context)
        {
            var optimized = await _optimizer.OptimizeAsync(result.FinalPrompt, context.Goal, context.Context);

            Console.WriteLine("🧠 Optimized Prompt:");
            Console.WriteLine(optimized.OptimizedPrompt);
            Console.WriteLine("💡 Suggestions:");
            foreach (var s in optimized.Suggestions)
            {
                Console.WriteLine("- " + s);
            }

            // Optionally overwrite:
            // result.FinalPrompt = optimized.OptimizedPrompt;
        }
    }
}
