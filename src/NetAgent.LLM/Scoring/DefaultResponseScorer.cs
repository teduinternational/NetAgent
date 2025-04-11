using NetAgent.Abstractions.LLM;
using System.Text.RegularExpressions;

namespace NetAgent.LLM.Scoring
{
    public class DefaultResponseScorer : IResponseScorer
    {
        private static readonly string[] CodeIndicators = new[] { "```", "    ", "public", "private", "class", "function", "def", "var", "const" };
        private static readonly string[] QualityIndicators = new[] { "example", "explanation", "steps", "here's", "consider" };
        
        public double ScoreResponse(string response)
        {
            if (string.IsNullOrEmpty(response)) return 0;

            double score = 0;
            var normalizedResponse = response.Trim();
            
            // Length score (0-30 points)
            score += Math.Min(normalizedResponse.Length / 200.0, 30);
            
            // Code quality (0-25 points)
            score += CalculateCodeQualityScore(normalizedResponse);
            
            // Content quality (0-25 points)
            score += CalculateContentQualityScore(normalizedResponse);
            
            // Formatting quality (0-20 points)
            score += CalculateFormattingScore(normalizedResponse);

            return Math.Min(Math.Max(score, 0), 100);
        }

        private double CalculateCodeQualityScore(string response)
        {
            double score = 0;
            
            // Code block presence and quality
            score += CodeIndicators.Count(i => response.Contains(i)) * 2.5;
            
            // Check for proper code block closure
            var codeBlockMatches = Regex.Matches(response, "```.*?```", RegexOptions.Singleline);
            score += codeBlockMatches.Count * 5;
            
            return Math.Min(score, 25);
        }

        private double CalculateContentQualityScore(string response)
        {
            double score = 0;
            
            // Quality indicators presence
            score += QualityIndicators.Count(i => response.Contains(i, StringComparison.OrdinalIgnoreCase)) * 3;
            
            // Sentence structure
            var sentences = response.Split('.').Where(s => s.Length > 10).ToList();
            score += Math.Min(sentences.Count * 2, 10);
            
            // Technical term density
            score += CalculateTechnicalTermDensity(response);
            
            return Math.Min(score, 25);
        }

        private double CalculateFormattingScore(string response)
        {
            double score = 0;
            
            // Proper line breaks (not too many, not too few)
            var lineBreaks = response.Count(c => c == '\n');
            score += Math.Min(lineBreaks * 0.5, 10);
            
            // Markdown structure quality
            score += response.Contains("#") ? 5 : 0;
            score += !Regex.IsMatch(response, @"\n{4,}") ? 5 : 0;
            
            return Math.Min(score, 20);
        }

        private double CalculateTechnicalTermDensity(string response)
        {
            var words = response.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var technicalTerms = words.Count(w => 
                w.Contains("(") || w.Contains(")") || 
                w.Contains(".") || w.Contains("_") ||
                CodeIndicators.Any(i => w.Contains(i)));
            
            return Math.Min((double)technicalTerms / words.Length * 10, 5);
        }
    }
}
