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
using Moq;
using Xunit;

namespace NetAgent.Runtime.Tests.Agents
{
    public class MCPAgentFactoryTests
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IMultiLLMProvider> _mockMultiLLM;
        private readonly Mock<IAgentTool> _mockTool;
        private readonly Mock<IKeyValueMemoryStore> _mockMemory;
        private readonly Mock<IAgentPlanner> _mockPlanner;
        private readonly Mock<IContextSource> _mockContextSource;
        private readonly Mock<IAgentPostProcessor> _mockPostProcessor;
        private readonly Mock<IAgentStrategy> _mockStrategy;
        private readonly Mock<IEvaluator> _mockEvaluator;
        private readonly Mock<IOptimizer> _mockOptimizer;

        public MCPAgentFactoryTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockMultiLLM = new Mock<IMultiLLMProvider>();
            _mockTool = new Mock<IAgentTool>();
            _mockMemory = new Mock<IKeyValueMemoryStore>();
            _mockPlanner = new Mock<IAgentPlanner>();
            _mockContextSource = new Mock<IContextSource>();
            _mockPostProcessor = new Mock<IAgentPostProcessor>();
            _mockStrategy = new Mock<IAgentStrategy>();
            _mockEvaluator = new Mock<IEvaluator>();
            _mockOptimizer = new Mock<IOptimizer>();

            // Setup basic service provider
            _mockServiceProvider.Setup(x => x.GetService(typeof(IMultiLLMProvider)))
                .Returns(_mockMultiLLM.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IEnumerable<IAgentTool>)))
                .Returns(new[] { _mockTool.Object });
            _mockServiceProvider.Setup(x => x.GetService(typeof(IKeyValueMemoryStore)))
                .Returns(_mockMemory.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IAgentPlanner)))
                .Returns(_mockPlanner.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IContextSource)))
                .Returns(_mockContextSource.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IAgentPostProcessor)))
                .Returns(_mockPostProcessor.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IAgentStrategy)))
                .Returns(_mockStrategy.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IEvaluator)))
                .Returns(_mockEvaluator.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IOptimizer)))
                .Returns(_mockOptimizer.Object);
        }

        [Fact]
        public async Task CreateAgent_ShouldCreateValidAgent()
        {
            // Arrange
            var factory = new MCPAgentFactory(_mockServiceProvider.Object);
            var options = new AgentOptions();

            // Act
            var agent = await factory.CreateAgent(options);

            // Assert  
            Assert.NotNull(agent);
            Assert.IsType<MCPAgent>(agent);
        }
    }
}