using Moq;
using NetAgent.Abstractions;
using NetAgent.Abstractions.LLM;
using NetAgent.Abstractions.Models;
using NetAgent.Abstractions.Tools;
using NetAgent.Core.Contexts;
using NetAgent.Core.Memory;
using NetAgent.Core.Planning;
using NetAgent.Evaluation.Interfaces;
using NetAgent.LLM.Providers;
using NetAgent.Optimization.Interfaces;
using NetAgent.Runtime.Agents;
using NetAgent.Runtime.PostProcessing;
using NetAgent.Strategy;
using Xunit;

namespace NetAgent.Runtime.Tests.Agents
{
    public class MCPAgentBuilderTests
    {
        private readonly Mock<ILLMProvider> _mockLLM;
        private readonly Mock<IAgentTool> _mockTool1;
        private readonly Mock<IAgentTool> _mockTool2;
        private readonly Mock<IMultiLLMProvider> _mockMultiLLM;
        
        public MCPAgentBuilderTests()
        {
            _mockLLM = new Mock<ILLMProvider>();
            _mockTool1 = new Mock<IAgentTool>();
            _mockTool2 = new Mock<IAgentTool>();
            _mockMultiLLM = new Mock<IMultiLLMProvider>();

            _mockTool1.Setup(x => x.Name).Returns("Tool1");
            _mockTool2.Setup(x => x.Name).Returns("Tool2");
        }

        [Fact]
        public void Build_WithMultiLLMAndTools_ShouldCreateValidAgent()
        {
            // Arrange
            var tools = new[] { _mockTool1.Object, _mockTool2.Object };
            var options = new AgentOptions
            {
                Name = "TestAgent",
                EnabledTools = new List<string> { "Tool1", "Tool2" }
            };

            // Act
            var agent = new MCPAgentBuilder()
                .WithMultiLLM(_mockMultiLLM.Object)
                .WithTools(tools)
                .WithOptions(options)
                .Build();

            // Assert
            Assert.NotNull(agent);
            Assert.IsType<MCPAgent>(agent);
        }

        [Fact]
        public void Build_WithSingleLLMAndTools_ShouldCreateValidAgent()
        {
            // Arrange
            var tools = new[] { _mockTool1.Object };
            var options = new AgentOptions
            {
                Name = "TestAgent",
                EnabledTools = new List<string> { "Tool1" }
            };

            // Act
            var agent = new MCPAgentBuilder()
                .WithLLM(_mockLLM.Object)
                .WithTools(tools)
                .WithOptions(options)
                .Build();

            // Assert
            Assert.NotNull(agent);
            Assert.IsType<MCPAgent>(agent);
        }

        [Fact]
        public void Build_WithMultiProviderFallback_ShouldCreateValidAgent()
        {
            // Arrange
            var provider1 = new Mock<ILLMProvider>();
            var provider2 = new Mock<ILLMProvider>();
            provider1.Setup(x => x.Name).Returns("Provider1");
            provider2.Setup(x => x.Name).Returns("Provider2");

            _mockMultiLLM.Setup(x => x.GetProviders())
                .Returns(new[] { provider1.Object, provider2.Object });

            var options = new AgentOptions
            {
                PreferredProviders = new[] { "Provider1", "Provider2" }
            };

            // Act
            var agent = new MCPAgentBuilder()
                .WithMultiLLM(_mockMultiLLM.Object)
                .WithOptions(options)
                .Build();

            // Assert
            Assert.NotNull(agent);
            Assert.IsType<MCPAgent>(agent);
        }

        [Fact]
        public void Build_WithoutLLMProvider_ShouldThrowException()
        {
            // Arrange
            var builder = new MCPAgentBuilder();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }
    }
}