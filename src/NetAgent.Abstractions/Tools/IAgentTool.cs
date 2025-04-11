using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Tools
{
    public interface IAgentTool
    {
        string Name { get; }
        Task<string> ExecuteAsync(string input);
    }
}
