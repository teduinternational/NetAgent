namespace NetAgent.LLM.OpenAI
{
    public class OpenAIOptions
    {
        public string ApiKey { get; set; }
        public string Model { get; set; }
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }

        public string EmbeddingModel { set; get; }
    }
}
