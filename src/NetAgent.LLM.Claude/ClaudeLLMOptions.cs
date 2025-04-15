namespace NetAgent.LLM.Claude
{
    public class ClaudeLLMOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "claude-3-opus-20240229";
        public decimal? Temperature { get; set; } = 0.7m;
        public int? MaxTokens { get; set; } = 2000;
        public string EmbeddingModel { set; get; }

    }
}