namespace NetAgent.Abstractions.Models
{
    public class Prompt
    {
        public string RawInput { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string ToolOutput { get; set; } = string.Empty;

        public string FinalPrompt { get; set; } = string.Empty; // Prompt sau khi build hoàn chỉnh
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
