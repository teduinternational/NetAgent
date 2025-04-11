namespace NetAgent.LLM.Grok
{
    public class GrokOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "grok-1";
        public double? Temperature { get; set; } = 0.7;
        public int? MaxTokens { get; set; } = 2000;
    }
}