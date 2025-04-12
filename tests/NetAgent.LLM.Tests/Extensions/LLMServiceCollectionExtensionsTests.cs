using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NetAgent.Abstractions.LLM;
using NetAgent.LLM.Extensions;
using NetAgent.LLM.Providers;
using NetAgent.LLM.Scoring;

namespace NetAgent.LLM.Tests.Extensions
{
    public class LLMServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddMultiLLMProviders_ShouldRegisterRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<ILLMProvider>();
            var mockLogger = new Mock<ILogger<MultiLLMProvider>>();

            services.AddSingleton(mockProvider.Object);
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddMultiLLMProviders();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var multiProvider = serviceProvider.GetService<IMultiLLMProvider>();
            var scorer = serviceProvider.GetService<IResponseScorer>();

            Assert.NotNull(multiProvider);
            Assert.IsType<MultiLLMProvider>(multiProvider);
            Assert.NotNull(scorer);
            Assert.IsType<DefaultResponseScorer>(scorer);
        }

        [Fact]
        public void AddMultiLLMProviders_WithMultipleProviders_ShouldRegisterAll()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider1 = new Mock<ILLMProvider>();
            var mockProvider2 = new Mock<ILLMProvider>();
            var mockLogger = new Mock<ILogger<MultiLLMProvider>>();

            services.AddSingleton(mockProvider1.Object);
            services.AddSingleton(mockProvider2.Object);
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddMultiLLMProviders();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var multiProvider = serviceProvider.GetService<IMultiLLMProvider>();
            Assert.NotNull(multiProvider);

            var providers = multiProvider.GetProviders();
            Assert.Equal(2, providers.Count());
            Assert.Contains(mockProvider1.Object, providers);
            Assert.Contains(mockProvider2.Object, providers);
        }

        [Fact]
        public void AddMultiLLMProviders_WithoutProviders_ShouldStillRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLogger = new Mock<ILogger<MultiLLMProvider>>();
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddMultiLLMProviders();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var multiProvider = serviceProvider.GetService<IMultiLLMProvider>();
            var scorer = serviceProvider.GetService<IResponseScorer>();

            Assert.NotNull(multiProvider);
            Assert.NotNull(scorer);
            Assert.Empty(multiProvider.GetProviders());
        }

        [Fact]
        public void AddMultiLLMProviders_ShouldRegisterAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLogger = new Mock<ILogger<MultiLLMProvider>>();
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddMultiLLMProviders();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var instance1 = serviceProvider.GetService<IMultiLLMProvider>();
            var instance2 = serviceProvider.GetService<IMultiLLMProvider>();

            Assert.Same(instance1, instance2);
        }
    }
}