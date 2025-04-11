using Microsoft.Extensions.Options;

namespace NetAgent.Hosting.Options
{
    public class NetAgentOptionsValidator : IValidateOptions<NetAgentOptions>
    {
        public ValidateOptionsResult Validate(string name, NetAgentOptions options)
        {
            if (options == null)
            {
                return ValidateOptionsResult.Fail("Options cannot be null");
            }

            if (string.IsNullOrEmpty(options.LLM.Provider))
            {
                return ValidateOptionsResult.Fail("Provider must be specified");
            }

            // Add LLM provider specific validation
            switch (options.LLM.Provider.ToLowerInvariant())
            {
                case "openai":
                    if (options.LLM.OpenAI == null)
                    {
                        return ValidateOptionsResult.Fail("OpenAI configuration is required when using OpenAI provider");
                    }
                    if (string.IsNullOrEmpty(options.LLM.OpenAI.ApiKey))
                    {
                        return ValidateOptionsResult.Fail("OpenAI ApiKey is required");
                    }
                    break;

                case "azureopenai":
                    if (options.LLM.AzureOpenAI == null)
                    {
                        return ValidateOptionsResult.Fail("Azure OpenAI configuration is required when using Azure OpenAI provider");
                    }
                    if (string.IsNullOrEmpty(options.LLM.AzureOpenAI.Endpoint))
                    {
                        return ValidateOptionsResult.Fail("Azure OpenAI Endpoint is required");
                    }
                    if (string.IsNullOrEmpty(options.LLM.AzureOpenAI.ApiKey))
                    {
                        return ValidateOptionsResult.Fail("Azure OpenAI ApiKey is required");
                    }
                    break;

                case "ollama":
                    if (options.LLM.Ollama == null)
                    {
                        return ValidateOptionsResult.Fail("Ollama configuration is required when using Ollama provider");
                    }
                    if (string.IsNullOrEmpty(options.LLM.Ollama.Host))
                    {
                        return ValidateOptionsResult.Fail("Ollama Endpoint is required");
                    }
                    break;

                default:
                    return ValidateOptionsResult.Fail($"Unsupported provider: {options.LLM.Provider}");
            }

            return ValidateOptionsResult.Success;
        }
    }
}