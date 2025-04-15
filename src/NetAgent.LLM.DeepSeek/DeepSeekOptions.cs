namespace NetAgent.LLM.DeepSeek
{
    public class DeepSeekOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "deepseek-coder";
        public double? Temperature { get; set; } = 0.7;
        public int? MaxTokens { get; set; } = 2000;
        public string EmbeddingModel { set; get; }

    }
}