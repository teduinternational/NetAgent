namespace NetAgent.LLM.Gemini
{
    public class GeminiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-1.5-flash";
        public double? Temperature { get; set; } = 0.7;
        public int? MaxTokens { get; set; } = 2000;
        public string EmbeddingModel { set; get; }

    }
}