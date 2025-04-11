using NetAgent.Optimization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Optimization.Interfaces
{
    public interface IOptimizer
    {
        Task<OptimizationResult> OptimizeAsync(string prompt, string goal, string context);
    }
}
