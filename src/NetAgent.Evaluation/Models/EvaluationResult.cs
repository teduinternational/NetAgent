namespace NetAgent.Evaluation.Models
{
    public class EvaluationResult
    {
        public bool IsError { get; set; } = false;
        public double Score { get; set; }
        public bool IsAcceptable { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public IList<string> Improvements { get; set; } = new List<string>();
        public IDictionary<string, double> DetailedScores { get; set; } = new Dictionary<string, double>();
    }
}
