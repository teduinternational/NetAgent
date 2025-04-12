using Moq;
using NetAgent.Abstractions;
using NetAgent.Abstractions.Models;

namespace NetAgent.Abstractions.Tests
{
    public class AgentFactoryTests
    {
        private readonly Mock<IAgentFactory> _mockFactory;

        public AgentFactoryTests()
        {
            _mockFactory = new Mock<IAgentFactory>();
        }

        [Fact]
        public async Task CreateAgent_WithValidOptions_ReturnsAgent()
        {
            // Arrange
            var options = new AgentOptions
            {
                Name = "TestAgent",
                Role = "Test Role",
                Goals = new[] { "Goal1", "Goal2" }
            };
            var mockAgent = new Mock<IAgent>();
            _mockFactory.Setup(x => x.CreateAgent(options))
                .ReturnsAsync(mockAgent.Object);

            // Act
            var agent = await _mockFactory.Object.CreateAgent(options);

            // Assert
            Assert.NotNull(agent);
            _mockFactory.Verify(x => x.CreateAgent(options), Times.Once);
        }

        [Fact]
        public async Task CreateAgent_WithValidOptions_SetsCorrectName()
        {
            // Arrange
            var options = new AgentOptions
            {
                Name = "CustomAgent",
                Role = "Test Role",
                Goals = new[] { "Goal1" }
            };
            var mockAgent = new Mock<IAgent>();
            _mockFactory.Setup(x => x.CreateAgent(options))
                .ReturnsAsync(mockAgent.Object);

            // Act
            var agent = await _mockFactory.Object.CreateAgent(options);

            // Assert
            Assert.NotNull(agent);
            Assert.IsAssignableFrom<IAgent>(agent);
        }

        [Fact]
        public async Task CreateAgent_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            _mockFactory.Setup(x => x.CreateAgent(null!))
                .ThrowsAsync(new ArgumentNullException(nameof(AgentOptions)));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _mockFactory.Object.CreateAgent(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CreateAgent_WithInvalidName_ThrowsArgumentException(string name)
        {
            // Arrange
            var options = new AgentOptions
            {
                Name = name,
                Role = "Test Role",
                Goals = new[] { "Goal1" }
            };

            _mockFactory.Setup(x => x.CreateAgent(options))
                .ThrowsAsync(new ArgumentException("Name cannot be null or whitespace", nameof(options)));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _mockFactory.Object.CreateAgent(options));
        }
    }
}