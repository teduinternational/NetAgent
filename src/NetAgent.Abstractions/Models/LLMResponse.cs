namespace NetAgent.Abstractions.Models
{
    public class LLMResponse
    {
        public bool IsError { get; set; } = false;
        public string Content { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public long LatencyMs { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
