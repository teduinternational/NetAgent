using NetAgent.Abstractions.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAgent.Abstractions.Models
{
    /// <summary>
    /// Kết quả trả về từ agent sau khi hoàn tất xử lý
    /// </summary>
    public class AgentResponse
    {
        public string Output { get; set; } = string.Empty;
        public List<ToolResult> ToolTrace { get; set; } = new();
        public string? FinalPrompt { get; set; }
        public bool ContinueIteration { get; set; }
    }
}
