using Moq;
using NetAgent.Abstractions;
using NetAgent.Abstractions.Models;

namespace NetAgent.Abstractions.Tests
{
    public class AgentTests
    {
        private readonly Mock<IAgent> _mockAgent;

        public AgentTests()
        {
            _mockAgent = new Mock<IAgent>();
        }

        [Fact]
        public async Task ProcessAsync_WithValidRequest_ReturnsResponse()
        {
            // Arrange
            var request = new AgentRequest 
            { 
                Goal = "Test goal",
                InputContext = new AgentInputContext()
            };
            var expectedResponse = new AgentResponse();
            _mockAgent.Setup(x => x.ProcessAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _mockAgent.Object.ProcessAsync(request);

            // Assert
            Assert.NotNull(response);
            _mockAgent.Verify(x => x.ProcessAsync(request), Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            _mockAgent.Setup(x => x.ProcessAsync(null!))
                .ThrowsAsync(new ArgumentNullException(nameof(AgentRequest)));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _mockAgent.Object.ProcessAsync(null!));
        }

        [Fact]
        public async Task ProcessAsync_WithEmptyGoal_ShouldStillExecute()
        {
            // Arrange
            var request = new AgentRequest 
            { 
                Goal = string.Empty,
                InputContext = new AgentInputContext()
            };
            var expectedResponse = new AgentResponse();
            _mockAgent.Setup(x => x.ProcessAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _mockAgent.Object.ProcessAsync(request);

            // Assert
            Assert.NotNull(response);
        }
    }
}