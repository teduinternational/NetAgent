using Moq;
using NetAgent.Abstractions;
using NetAgent.Abstractions.Tools;

namespace NetAgent.Abstractions.Tests
{
    public class ToolPluginTests
    {
        private readonly Mock<IToolPlugin> _mockToolPlugin;

        public ToolPluginTests()
        {
            _mockToolPlugin = new Mock<IToolPlugin>();
        }

        [Fact]
        public void Name_ShouldNotBeNullOrEmpty()
        {
            // Arrange
            _mockToolPlugin.Setup(x => x.Name)
                .Returns("TestPlugin");

            // Act
            var name = _mockToolPlugin.Object.Name;

            // Assert
            Assert.NotNull(name);
            Assert.NotEmpty(name);
            Assert.Equal("TestPlugin", name);
        }

        [Fact]
        public void GetTools_ShouldReturnNonNullCollection()
        {
            // Arrange
            var mockTool = new Mock<IAgentTool>();
            _mockToolPlugin.Setup(x => x.GetTools())
                .Returns(new[] { mockTool.Object });

            // Act
            var tools = _mockToolPlugin.Object.GetTools();

            // Assert
            Assert.NotNull(tools);
            Assert.Collection(tools, tool => Assert.NotNull(tool));
        }

        [Fact]
        public void GetTools_WhenNoTools_ShouldReturnEmptyCollection()
        {
            // Arrange
            _mockToolPlugin.Setup(x => x.GetTools())
                .Returns(Array.Empty<IAgentTool>());

            // Act
            var tools = _mockToolPlugin.Object.GetTools();

            // Assert
            Assert.NotNull(tools);
            Assert.Empty(tools);
        }
    }
}