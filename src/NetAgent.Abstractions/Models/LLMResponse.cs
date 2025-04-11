namespace NetAgent.Abstractions.Models
{
    public class LLMResponse
    {
        public string GeneratedText { get; set; } = string.Empty;
        public string UsedPrompt { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public long LatencyMs { get; set; }
        public double? Score { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
