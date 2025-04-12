using NetAgent.Abstractions.LLM;
using System.Text.RegularExpressions;

namespace NetAgent.LLM.Scoring
{
    public class DefaultResponseScorer : IResponseScorer
    {
        private static readonly string[] CodeIndicators = new[] { "```", "    ", "public", "private", "class", "function", "def", "var", "const" };
        private static readonly string[] QualityIndicators = new[] { "example", "explanation", "steps", "here's", "consider" };
        private static readonly string[] ErrorIndicators = new[] { "error", "exception", "sorry", "can't help", "cannot help", "failed" };
        
        public double ScoreResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) return 0;

            // Check for error indicators first
            if (ErrorIndicators.Any(e => response.Contains(e, StringComparison.OrdinalIgnoreCase)))
            {
                return 0;
            }

            double score = 0;
            var normalizedResponse = response.Trim();
            
            // Length score (0-0.3)
            score += Math.Min(normalizedResponse.Length / 20000.0, 0.3);
            
            // Code quality (0-0.25)
            score += CalculateCodeQualityScore(normalizedResponse);
            
            // Content quality (0-0.25)
            score += CalculateContentQualityScore(normalizedResponse);
            
            // Formatting quality (0-0.2)
            score += CalculateFormattingScore(normalizedResponse);

            return Math.Min(Math.Max(score, 0), 1.0);
        }

        private double CalculateCodeQualityScore(string response)
        {
            double score = 0;
            
            // Code block presence and quality
            score += CodeIndicators.Count(i => response.Contains(i)) * 0.025;
            
            // Check for proper code block closure
            var codeBlockMatches = Regex.Matches(response, "```.*?```", RegexOptions.Singleline);
            score += codeBlockMatches.Count * 0.05;
            
            return Math.Min(score, 0.25);
        }

        private double CalculateContentQualityScore(string response)
        {
            double score = 0;
            
            // Quality indicators presence
            score += QualityIndicators.Count(i => response.Contains(i, StringComparison.OrdinalIgnoreCase)) * 0.03;
            
            // Sentence structure
            var sentences = response.Split('.').Where(s => s.Length > 10).ToList();
            score += Math.Min(sentences.Count * 0.02, 0.1);
            
            // Technical term density
            score += CalculateTechnicalTermDensity(response);
            
            return Math.Min(score, 0.25);
        }

        private double CalculateFormattingScore(string response)
        {
            double score = 0;
            
            // Proper line breaks (not too many, not too few)
            var lineBreaks = response.Count(c => c == '\n');
            score += Math.Min(lineBreaks * 0.005, 0.1);
            
            // Markdown structure quality
            score += response.Contains("#") ? 0.05 : 0;
            score += !Regex.IsMatch(response, @"\n{4,}") ? 0.05 : 0;
            
            return Math.Min(score, 0.2);
        }

        private double CalculateTechnicalTermDensity(string response)
        {
            var words = response.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return 0;

            var technicalTerms = words.Count(w => 
                w.Contains("(") || w.Contains(")") || 
                w.Contains(".") || w.Contains("_") ||
                CodeIndicators.Any(i => w.Contains(i)));
            
            return Math.Min((double)technicalTerms / words.Length * 0.1, 0.05);
        }
    }
}
