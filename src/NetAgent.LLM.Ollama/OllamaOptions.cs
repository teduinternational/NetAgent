namespace NetAgent.LLM.Ollama
{
    public class OllamaOptions
    {
        public string Host { get; set; } = "http://localhost:11434"; // Mặc định Ollama API
        public string Model { get; set; } = "llama3"; // Có thể là mistral, codellama, v.v.
    }
}
