namespace NetAgent.LLM.AzureOpenAI
{
    public class AzureOpenAIOptions
    {
        public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
        public string ApiKey { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty; // e.g. my-aoai
        public string DeploymentName { get; set; } = string.Empty; // e.g. gpt-35-turbo
        public string ApiVersion { get; set; } = "2023-07-01-preview";
    }
}
