using NetAgent.Abstractions.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions
{
    /// <summary>
    /// Interface cho một công cụ mở rộng (plugin) có thể tích hợp vào agent
    /// </summary>
    public interface IToolPlugin
    {
        string Name { get; }
        IEnumerable<IAgentTool> GetTools();
    }
}
