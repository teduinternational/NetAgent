using Microsoft.Extensions.Options;
using NetAgent.Hosting.Options;
using NetAgent.LLM.AzureOpenAI;
using NetAgent.LLM.Ollama;
using NetAgent.LLM.OpenAI;
using Xunit;

namespace NetAgent.Core.Tests.Options
{
    public class NetAgentOptionsValidatorTests
    {
        private readonly NetAgentOptionsValidator _validator;

        public NetAgentOptionsValidatorTests()
        {
            _validator = new NetAgentOptionsValidator();
        }

        [Fact]
        public void Validate_WithNullOptions_ShouldReturnFail()
        {
            // Act
            var result = _validator.Validate("test", null);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Options cannot be null", result.FailureMessage);
        }

        [Fact]
        public void Validate_WithEmptyProvider_ShouldReturnFail()
        {
            // Arrange
            var options = new NetAgentOptions 
            { 
                LLM = new LLMOptions { Provider = "" } 
            };

            // Act
            var result = _validator.Validate("test", options);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Provider must be specified", result.FailureMessage);
        }

        [Fact]
        public void Validate_WithValidOpenAIConfig_ShouldReturnSuccess()
        {
            // Arrange
            var options = new NetAgentOptions
            {
                LLM = new LLMOptions
                {
                    Provider = "openai",
                    OpenAI = new OpenAIOptions
                    {
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    }
                }
            };

            // Act
            var result = _validator.Validate("test", options);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public void Validate_WithMissingOpenAIApiKey_ShouldReturnFail()
        {
            // Arrange
            var options = new NetAgentOptions
            {
                LLM = new LLMOptions
                {
                    Provider = "openai",
                    OpenAI = new OpenAIOptions
                    {
                        Model = "gpt-4"
                    }
                }
            };

            // Act
            var result = _validator.Validate("test", options);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("OpenAI ApiKey is required", result.FailureMessage);
        }

        [Fact]
        public void Validate_WithValidAzureOpenAIConfig_ShouldReturnSuccess()
        {
            // Arrange
            var options = new NetAgentOptions
            {
                LLM = new LLMOptions
                {
                    Provider = "azureopenai",
                    AzureOpenAI = new AzureOpenAIOptions
                    {
                        Endpoint = "https://test.openai.azure.com",
                        ApiKey = "test-key",
                        DeploymentName = "test-deployment"
                    }
                }
            };

            // Act
            var result = _validator.Validate("test", options);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public void Validate_WithValidOllamaConfig_ShouldReturnSuccess()
        {
            // Arrange
            var options = new NetAgentOptions
            {
                LLM = new LLMOptions
                {
                    Provider = "ollama",
                    Ollama = new OllamaOptions
                    {
                        Host = "http://localhost:11434",
                        Model = "llama2"
                    }
                }
            };

            // Act
            var result = _validator.Validate("test", options);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public void Validate_WithInvalidProvider_ShouldReturnFail()
        {
            // Arrange
            var options = new NetAgentOptions
            {
                LLM = new LLMOptions { Provider = "invalid" }
            };

            // Act
            var result = _validator.Validate("test", options);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Unsupported provider: invalid", result.FailureMessage);
        }
    }
}