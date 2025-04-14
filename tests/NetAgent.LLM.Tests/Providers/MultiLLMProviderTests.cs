using Microsoft.Extensions.Logging;
using Moq;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.LLM.Monitoring;
using NetAgent.LLM.Providers;

namespace NetAgent.LLM.Tests.Providers
{
    public class MultiLLMProviderTests
    {
        private readonly Mock<ILLMProvider> _mockProvider1;
        private readonly Mock<ILLMProvider> _mockProvider2;
        private readonly Mock<IResponseScorer> _mockScorer;
        private readonly Mock<ILogger<IMultiLLMProvider>> _mockLogger;
        private readonly MultiLLMProvider _provider;
        private readonly Mock<ILLMHealthCheck> _mockHealthCheck;
        private readonly Mock<ILLMPreferences> _mockPreferences;

        public MultiLLMProviderTests()
        {
            _mockProvider1 = new Mock<ILLMProvider>();
            _mockProvider1.Setup(x => x.Name).Returns("Provider1");
            
            _mockProvider2 = new Mock<ILLMProvider>();
            _mockProvider2.Setup(x => x.Name).Returns("Provider2");
            
            _mockScorer = new Mock<IResponseScorer>();
            _mockLogger = new Mock<ILogger<IMultiLLMProvider>>();
            _mockPreferences = new Mock<ILLMPreferences>();
            _mockHealthCheck = new Mock<ILLMHealthCheck>();

            var providers = new[] { _mockProvider1.Object, _mockProvider2.Object };
            _provider = new MultiLLMProvider(providers, _mockScorer.Object, 
                _mockLogger.Object, _mockHealthCheck.Object, _mockPreferences.Object);
        }

        [Fact]
        public void Name_ShouldReturnMultiLLM()
        {
            Assert.Equal("MultiLLM", _provider.Name);
        }

        [Fact]
        public async Task GenerateFromAllAsync_ShouldReturnAllSuccessfulResponses()
        {
            // Arrange
            var prompt = new Prompt("test");
            var response1 = new LLMResponse { Content = "response1" };
            var response2 = new LLMResponse { Content = "response2" };

            _mockProvider1.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(response1);
            _mockProvider2.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(response2);
            _mockPreferences.Setup(x => x.IsProviderAllowed(It.IsAny<string>())).Returns(true);

            // Act
            var responses = await _provider.GenerateFromAllAsync(prompt);

            // Assert
            Assert.Equal(2, responses.Length);
            Assert.Contains(response1, responses);
            Assert.Contains(response2, responses);
        }

        [Fact]
        public async Task GenerateFromAllAsync_ShouldHandleFailedProviders()
        {
            // Arrange
            var prompt = new Prompt("test");
            var response2 = new LLMResponse { Content = "response2" };

            _mockProvider1.Setup(x => x.GenerateAsync(prompt))
                .ThrowsAsync(new LLMException("Provider failed"));
            _mockProvider2.Setup(x => x.GenerateAsync(prompt))
                .ReturnsAsync(response2);
            _mockPreferences.Setup(x => x.IsProviderAllowed(It.IsAny<string>())).Returns(true);

            // Act
            var responses = await _provider.GenerateFromAllAsync(prompt);

            // Assert
            Assert.Single(responses);
            Assert.Equal(response2, responses[0]);
        }

        [Fact]
        public async Task GenerateAsync_ShouldReturnFirstSuccessfulResponse()
        {
            // Arrange
            var prompt = new Prompt("test");
            var response1 = new LLMResponse { Content = "response1" };

            _mockProvider1.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(response1);
            _mockPreferences.Setup(x => x.IsProviderAllowed(It.IsAny<string>())).Returns(true);

            // Act
            var response = await _provider.GenerateAsync(prompt);

            // Assert
            Assert.Equal(response1, response);
            _mockProvider2.Verify(x => x.GenerateAsync(prompt), Times.Never);
        }

        [Fact]
        public async Task GenerateBestAsync_ShouldReturnHighestScoringResponse()
        {
            // Arrange
            var prompt = new Prompt("test");
            var response1 = new LLMResponse { Content = "response1" };
            var response2 = new LLMResponse { Content = "response2" };

            _mockProvider1.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(response1);
            _mockProvider2.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(response2);
            _mockPreferences.Setup(x => x.IsProviderAllowed(It.IsAny<string>())).Returns(true);

            _mockScorer.Setup(x => x.ScoreResponse(response1.Content)).Returns(0.5);
            _mockScorer.Setup(x => x.ScoreResponse(response2.Content)).Returns(0.8);

            // Act
            var bestResponse = await _provider.GenerateBestAsync(prompt);

            // Assert
            Assert.Equal(response2, bestResponse);
        }

        [Fact]
        public void GetProviders_ShouldReturnAllProviders()
        {
            // Act
            var providers = _provider.GetProviders();

            // Assert
            Assert.Equal(2, providers.Count());
            Assert.Contains(_mockProvider1.Object, providers);
            Assert.Contains(_mockProvider2.Object, providers);
        }

        [Fact]
        public void GetScorer_ShouldReturnConfiguredScorer()
        {
            // Act
            var scorer = _provider.GetScorer();

            // Assert
            Assert.Equal(_mockScorer.Object, scorer);
        }

        [Fact]
        public void GetLogger_ShouldReturnConfiguredLogger()
        {
            // Act
            var logger = _provider.GetLogger();

            // Assert
            Assert.Equal(_mockLogger.Object, logger);
        }

        [Fact]
        public async Task GenerateAsync_ShouldRespectProviderPreferences()
        {
            // Arrange
            var prompt = new Prompt("test");
            _mockPreferences.Setup(x => x.IsProviderAllowed("Provider1")).Returns(false);
            _mockPreferences.Setup(x => x.IsProviderAllowed("Provider2")).Returns(true);

            var response2 = new LLMResponse { Content = "response2" };
            _mockProvider2.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(response2);

            // Act
            var response = await _provider.GenerateAsync(prompt);

            // Assert
            Assert.Equal(response2, response);
            _mockProvider1.Verify(x => x.GenerateAsync(prompt), Times.Never);
        }

        [Fact]
        public async Task GenerateFromAllAsync_WhenAllProvidersFail_ShouldThrowException()
        {
            // Arrange
            var prompt = new Prompt("test");
            _mockProvider1.Setup(x => x.GenerateAsync(prompt))
                .ThrowsAsync(new LLMException("Provider1 failed"));
            _mockProvider2.Setup(x => x.GenerateAsync(prompt))
                .ThrowsAsync(new LLMException("Provider2 failed"));
            _mockPreferences.Setup(x => x.IsProviderAllowed(It.IsAny<string>())).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<LLMException>(() => _provider.GenerateFromAllAsync(prompt));
        }

        [Fact]
        public async Task GenerateAsync_ShouldRespectRetryTimeout()
        {
            // Arrange
            var prompt = new Prompt("test");
            _mockProvider1.Setup(x => x.GenerateAsync(prompt))
                .ThrowsAsync(new LLMException("Provider1 failed"));
            _mockProvider2.Setup(x => x.GenerateAsync(prompt))
                .ThrowsAsync(new LLMException("Provider2 failed"));
            _mockPreferences.Setup(x => x.IsProviderAllowed(It.IsAny<string>())).Returns(true);

            // Act - First call will fail
            await Assert.ThrowsAsync<AggregateException>(() => _provider.GenerateAsync(prompt));

            // Assert - Immediate retry should skip both failed providers and throw LLMException
            await Assert.ThrowsAsync<LLMException>(() => _provider.GenerateAsync(prompt));
            _mockProvider1.Verify(x => x.GenerateAsync(prompt), Times.Once);
            _mockProvider2.Verify(x => x.GenerateAsync(prompt), Times.Once);
        }

        [Fact]
        public async Task GenerateAsync_ShouldRespectProviderWeights()
        {
            // Arrange
            var prompt = new Prompt("test");
            _mockPreferences.Setup(x => x.IsProviderAllowed(It.IsAny<string>())).Returns(true);
            _mockPreferences.Setup(x => x.GetProviderWeight("Provider1")).Returns(1.0);
            _mockPreferences.Setup(x => x.GetProviderWeight("Provider2")).Returns(0.5);

            var response1 = new LLMResponse { Content = "response1" };
            var response2 = new LLMResponse { Content = "response2" };

            _mockProvider1.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(response1);
            _mockProvider2.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(response2);

            // Act
            var response = await _provider.GenerateAsync(prompt);

            // Assert
            Assert.Equal(response1, response);
            _mockProvider2.Verify(x => x.GenerateAsync(prompt), Times.Never);
        }

        [Fact]
        public async Task GenerateFromAllAsync_ShouldHandleEmptyResponses()
        {
            // Arrange
            var prompt = new Prompt("test");
            var emptyResponse = new LLMResponse();
            var validResponse = new LLMResponse { Content = "valid response" };

            _mockProvider1.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(emptyResponse);
            _mockProvider2.Setup(x => x.GenerateAsync(prompt)).ReturnsAsync(validResponse);
            _mockPreferences.Setup(x => x.IsProviderAllowed(It.IsAny<string>())).Returns(true);

            // Act
            var responses = await _provider.GenerateFromAllAsync(prompt);

            // Assert
            Assert.Equal(2, responses.Length);
            Assert.Contains(responses, r => string.IsNullOrEmpty(r.Content));
            Assert.Contains(responses, r => r.Content == "valid response");
        }
    }
}