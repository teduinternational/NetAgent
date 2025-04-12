namespace NetAgent.Abstractions.Models
{
    public class Prompt
    {
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();

        public Prompt(string content = "")
        {
            Content = content;
        }
    }
}
