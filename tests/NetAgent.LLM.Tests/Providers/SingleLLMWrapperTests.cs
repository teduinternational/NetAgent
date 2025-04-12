using Moq;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.LLM.Providers;
using NetAgent.LLM.Scoring;

namespace NetAgent.LLM.Tests.Providers
{
    public class SingleLLMWrapperTests
    {
        private readonly Mock<ILLMProvider> _mockProvider;
        private readonly SingleLLMWrapper _wrapper;

        public SingleLLMWrapperTests()
        {
            _mockProvider = new Mock<ILLMProvider>();
            _mockProvider.Setup(x => x.Name).Returns("TestProvider");
            _wrapper = new SingleLLMWrapper(_mockProvider.Object);
        }

        [Fact]
        public void Name_ShouldReturnProviderName()
        {
            // Act
            var name = _wrapper.Name;

            // Assert
            Assert.Equal("TestProvider", name);
        }

        [Fact]
        public async Task GenerateAsync_ShouldCallUnderlyingProvider()
        {
            // Arrange
            var prompt = new Prompt("test");
            var expectedResponse = new LLMResponse { Content = "test response" };
            _mockProvider.Setup(x => x.GenerateAsync(prompt))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _wrapper.GenerateAsync(prompt);

            // Assert
            Assert.Equal(expectedResponse, response);
            _mockProvider.Verify(x => x.GenerateAsync(prompt), Times.Once);
        }

        [Fact]
        public async Task GenerateAsync_ShouldPropagateExceptions()
        {
            // Arrange
            var prompt = new Prompt("test");
            var expectedMessage = "Test exception";
            _mockProvider.Setup(x => x.GenerateAsync(prompt))
                .ThrowsAsync(new LLMException(expectedMessage));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<LLMException>(
                () => _wrapper.GenerateAsync(prompt));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public async Task GenerateAsync_ShouldRespectCancellation()
        {
            // Arrange
            var prompt = new Prompt("test");
            var cts = new CancellationTokenSource();
            _mockProvider.Setup(x => x.GenerateAsync(prompt))
                .Returns(async () =>
                {
                    await Task.Delay(1000, cts.Token);
                    return new LLMResponse { Content = "response" };
                });

            // Act
            cts.Cancel();

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _wrapper.GenerateAsync(prompt));
        }

        [Fact]
        public async Task GenerateAsync_ShouldHandleTimeout()
        {
            // Arrange
            var prompt = new Prompt("test");
            _mockProvider.Setup(x => x.GenerateAsync(prompt))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new LLMResponse { Content = "response" };
                });

            // Act & Assert
            var response = await _wrapper.GenerateAsync(prompt);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GenerateFromAllAsync_ShouldReturnSingleResponse()
        {
            // Arrange
            var prompt = new Prompt("test");
            var expectedResponse = new LLMResponse { Content = "test response" };
            _mockProvider.Setup(x => x.GenerateAsync(prompt))
                .ReturnsAsync(expectedResponse);

            // Act
            var responses = await _wrapper.GenerateFromAllAsync(prompt);

            // Assert
            Assert.Single(responses);
            Assert.Equal(expectedResponse, responses[0]);
        }

        [Fact]
        public async Task GenerateFromAllAsync_ShouldPropagateExceptions()
        {
            // Arrange
            var prompt = new Prompt("test");
            var expectedMessage = "Test exception";
            _mockProvider.Setup(x => x.GenerateAsync(prompt))
                .ThrowsAsync(new LLMException(expectedMessage));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<LLMException>(
                () => _wrapper.GenerateFromAllAsync(prompt));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public async Task GenerateBestAsync_ShouldReturnProviderResponse()
        {
            // Arrange
            var prompt = new Prompt("test");
            var expectedResponse = new LLMResponse { Content = "test response" };
            _mockProvider.Setup(x => x.GenerateAsync(prompt))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _wrapper.GenerateBestAsync(prompt);

            // Assert
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task GenerateBestAsync_WithEmptyResponse_ShouldReturnEmptyResponse()
        {
            // Arrange
            var prompt = new Prompt("test");
            var emptyResponse = new LLMResponse();
            _mockProvider.Setup(x => x.GenerateAsync(prompt))
                .ReturnsAsync(emptyResponse);

            // Act
            var response = await _wrapper.GenerateBestAsync(prompt);

            // Assert
            Assert.NotNull(response);
            Assert.Empty(response.Content);
        }

        [Fact]
        public void GetProviders_ShouldReturnSingleProvider()
        {
            // Act
            var providers = _wrapper.GetProviders();

            // Assert
            Assert.Single(providers);
            Assert.Equal(_mockProvider.Object, providers.First());
        }

        [Fact]
        public void GetScorer_ShouldReturnDefaultScorer()
        {
            // Act
            var scorer = _wrapper.GetScorer();

            // Assert
            Assert.IsType<DefaultResponseScorer>(scorer);
        }

        [Fact]
        public void GetLogger_ShouldReturnNull()
        {
            // Act
            var logger = _wrapper.GetLogger();

            // Assert
            Assert.Null(logger);
        }
    }
}