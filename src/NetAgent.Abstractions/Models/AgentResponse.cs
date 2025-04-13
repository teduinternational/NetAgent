using NetAgent.Abstractions.Tools;

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
        public double EvaluationScore { get; set; }
        public string? Plan { get; set; }
        public string? Decision { get; set; }
    }
}
