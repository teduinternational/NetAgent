using NetAgent.LLM.Scoring;

namespace NetAgent.LLM.Tests.Scoring
{
    public class DefaultResponseScorerTests
    {
        private readonly DefaultResponseScorer _scorer;

        public DefaultResponseScorerTests()
        {
            _scorer = new DefaultResponseScorer();
        }

        [Theory]
        [InlineData(null, 0.0)]
        [InlineData("", 0.0)]
        [InlineData(" ", 0.0)]
        public void ScoreResponse_WithInvalidInput_ShouldReturnZero(string response, double expected)
        {
            // Act
            var score = _scorer.ScoreResponse(response);

            // Assert
            Assert.Equal(expected, score);
        }

        [Fact]
        public void ScoreResponse_WithValidResponse_ShouldReturnPositiveScore()
        {
            // Arrange
            var response = "This is a valid response";

            // Act
            var score = _scorer.ScoreResponse(response);

            // Assert
            Assert.True(score > 0.0);
            Assert.True(score <= 1.0);
        }

        [Fact]
        public void ScoreResponse_WithLongerResponse_ShouldReturnHigherScore()
        {
            // Arrange
            var shortResponse = "Short answer";
            var longResponse = "This is a longer, more detailed response that provides more information and context";

            // Act
            var shortScore = _scorer.ScoreResponse(shortResponse);
            var longScore = _scorer.ScoreResponse(longResponse);

            // Assert
            Assert.True(longScore > shortScore);
        }

        [Theory]
        [InlineData("Error occurred", 0.0)]
        [InlineData("Sorry, I can't help with that", 0.0)]
        [InlineData("An exception was thrown", 0.0)]
        public void ScoreResponse_WithErrorIndicators_ShouldReturnZero(string response, double expected)
        {
            // Act
            var score = _scorer.ScoreResponse(response);

            // Assert
            Assert.Equal(expected, score);
        }
    }
}