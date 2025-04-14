using Microsoft.Extensions.Logging;
using Moq;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.LLM.Monitoring;
using NetAgent.LLM.Preferences;
using NetAgent.LLM.Providers;
using NetAgent.LLM.Scoring;
using Xunit;

namespace NetAgent.Runtime.Tests.LLM
{
    public class MultiLLMProviderTests
    {
        private readonly Mock<ILLMProvider> _mockProvider1;
        private readonly Mock<ILLMProvider> _mockProvider2;
        private readonly Mock<IResponseScorer> _mockScorer;
        private readonly Mock<ILogger<MultiLLMProvider>> _mockLogger;
        private readonly Mock<ILLMHealthCheck> _mockHealthCheck;
        private readonly Mock<ILLMPreferences> _mockPreferences;

        public MultiLLMProviderTests()
        {
            _mockProvider1 = new Mock<ILLMProvider>();
            _mockProvider2 = new Mock<ILLMProvider>();
            _mockScorer = new Mock<IResponseScorer>();
            _mockLogger = new Mock<ILogger<MultiLLMProvider>>();
            _mockHealthCheck = new Mock<ILLMHealthCheck>();
            _mockPreferences = new Mock<ILLMPreferences>();
            // Setup provider names
            _mockProvider1.Setup(x => x.Name).Returns("Provider1");
            _mockProvider2.Setup(x => x.Name).Returns("Provider2");
        }

        [Fact]
        public async Task GenerateAsync_WhenFirstProviderFails_ShouldFallbackToSecond()
        {
            // Arrange
            var providers = new[] { _mockProvider1.Object, _mockProvider2.Object };
            var multiProvider = new MultiLLMProvider(providers, _mockScorer.Object,
                _mockLogger.Object,_mockHealthCheck.Object, _mockPreferences.Object);
            var prompt = new Prompt { Content = "Test prompt" };

            _mockProvider1
                .Setup(x => x.GenerateAsync(It.IsAny<Prompt>()))
                .ThrowsAsync(new Exception("Provider 1 failed"));

            _mockProvider2
                .Setup(x => x.GenerateAsync(It.IsAny<Prompt>()))
                .ReturnsAsync(new LLMResponse { Content = "Success from Provider 2" });

            // Act
            var result = await multiProvider.GenerateAsync(prompt);

            // Assert
            Assert.Equal("Success from Provider 2", result.Content);
            _mockProvider1.Verify(x => x.GenerateAsync(It.IsAny<Prompt>()), Times.Once);
            _mockProvider2.Verify(x => x.GenerateAsync(It.IsAny<Prompt>()), Times.Once);
        }

        [Fact]
        public async Task GenerateAsync_WhenAllProvidersFail_ShouldThrowException()
        {
            // Arrange
            var providers = new[] { _mockProvider1.Object, _mockProvider2.Object };
            var multiProvider = new MultiLLMProvider(providers, _mockScorer.Object, _mockLogger.Object,_mockHealthCheck.Object, _mockPreferences.Object);
            var prompt = new Prompt { Content = "Test prompt" };

            _mockProvider1
                .Setup(x => x.GenerateAsync(It.IsAny<Prompt>()))
                .ThrowsAsync(new Exception("Provider 1 failed"));

            _mockProvider2
                .Setup(x => x.GenerateAsync(It.IsAny<Prompt>()))
                .ThrowsAsync(new Exception("Provider 2 failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AggregateException>(() => multiProvider.GenerateAsync(prompt));
            _mockProvider1.Verify(x => x.GenerateAsync(It.IsAny<Prompt>()), Times.Once);
            _mockProvider2.Verify(x => x.GenerateAsync(It.IsAny<Prompt>()), Times.Once);
        }

        [Fact]
        public async Task GenerateAsync_WithPreferredProvider_ShouldTryPreferredFirst()
        {
            // Arrange
            var providers = new[] { _mockProvider1.Object, _mockProvider2.Object };
            var preferences = new LLMPreferences(new[] { "Provider2" });
            var multiProvider = new MultiLLMProvider(providers, _mockScorer.Object, _mockLogger.Object, _mockHealthCheck.Object, _mockPreferences.Object);
            var prompt = new Prompt { Content = "Test prompt" };

            _mockProvider2
                .Setup(x => x.GenerateAsync(It.IsAny<Prompt>()))
                .ReturnsAsync(new LLMResponse { Content = "Success from Provider 2" });

            // Act
            var result = await multiProvider.GenerateAsync(prompt);

            // Assert
            Assert.Equal("Success from Provider 2", result.Content);
            _mockProvider1.Verify(x => x.GenerateAsync(It.IsAny<Prompt>()), Times.Never);
            _mockProvider2.Verify(x => x.GenerateAsync(It.IsAny<Prompt>()), Times.Once);
        }

        [Fact]
        public async Task GenerateAsync_ShouldRespectRetryTimeout()
        {
            // Arrange
            var providers = new[] { _mockProvider1.Object, _mockProvider2.Object };
            var multiProvider = new MultiLLMProvider(providers, _mockScorer.Object, _mockLogger.Object, _mockHealthCheck.Object, _mockPreferences.Object);
            var prompt = new Prompt { Content = "Test prompt" };

            _mockProvider1
                .Setup(x => x.GenerateAsync(It.IsAny<Prompt>()))
                .ThrowsAsync(new Exception("Provider 1 failed"));
            _mockProvider2
                .Setup(x => x.GenerateAsync(It.IsAny<Prompt>()))
                .ThrowsAsync(new Exception("Provider 2 failed"));

            // Act - First attempt will cause both providers to fail
            await Assert.ThrowsAsync<AggregateException>(() => multiProvider.GenerateAsync(prompt));

            // Act - Second attempt should throw immediately without calling providers as they are in timeout
            await Assert.ThrowsAsync<LLMException>(() => multiProvider.GenerateAsync(prompt));

            // Assert - Verify providers were only called once during the first attempt
            _mockProvider1.Verify(x => x.GenerateAsync(It.IsAny<Prompt>()), Times.Once);
            _mockProvider2.Verify(x => x.GenerateAsync(It.IsAny<Prompt>()), Times.Once);
        }
    }
}