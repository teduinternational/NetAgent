using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions;
using NetAgent.Abstractions.Tools;
using NetAgent.Core.Exceptions;
using NetAgent.Core.Memory;
using NetAgent.Hosting.Extensions;
using NetAgent.Memory.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NetAgent.Core.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddNetAgentFromConfig_WithValidOpenAIConfig_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"NetAgent:Provider", "openai"},
                    {"NetAgent:OpenAI:ApiKey", "test-key"},
                    {"NetAgent:OpenAI:Model", "gpt-4"},
                    {"NetAgent:Tools:0", "calculator"}
                })
                .Build();

            // Act
            services.AddNetAgentFromConfig(config);

            // Assert
            var provider = services.BuildServiceProvider();
            var agent = provider.GetService<IAgent>();
            Assert.NotNull(agent);
        }

        [Fact]
        public void AddNetAgentFromConfig_WithMissingConfig_ShouldThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            // Act & Assert
            Assert.Throws<ConfigurationException>(() => services.AddNetAgentFromConfig(config));
        }

        [Fact]
        public void AddNetAgentFromConfig_WithInvalidProvider_ShouldThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"NetAgent:Provider", "invalid"}
                })
                .Build();

            // Act & Assert
            Assert.Throws<ConfigurationException>(() => services.AddNetAgentFromConfig(config));
        }

        [Fact]
        public void AddNetAgentFromConfig_WithMultipleTools_ShouldRegisterAllTools()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"NetAgent:Provider", "openai"},
                    {"NetAgent:OpenAI:ApiKey", "test-key"},
                    {"NetAgent:Tools:0", "calculator"},
                    {"NetAgent:Tools:1", "websearch"}
                })
                .Build();

            // Act
            services.AddNetAgentFromConfig(config);

            // Assert
            var provider = services.BuildServiceProvider();
            var tools = provider.GetServices<IAgentTool>().ToList();
            Assert.Equal(2, tools.Count);
        }

        [Fact]
        public void AddMemoryProviderFromConfig_WithDefaultConfig_ShouldUseInMemory()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            // Act
            services.AddMemoryProviderFromConfig(config);

            // Assert
            var provider = services.BuildServiceProvider();
            var memory = provider.GetService<IMemoryStore>();
            Assert.IsType<InMemoryMemoryStore>(memory);
        }
    }
}