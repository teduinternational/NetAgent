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

            if (options.LLM == null)
            {
                return ValidateOptionsResult.Fail("LLM options must be specified");
            }

            // Check if either Provider or Providers is specified
            if (string.IsNullOrEmpty(options.LLM.Provider) && (options.LLM.Providers == null || options.LLM.Providers.Length == 0))
            {
                return ValidateOptionsResult.Fail("Either Provider or Providers must be specified");
            }

            var providers = options.LLM.Providers ?? new[] { options.LLM.Provider! };
            foreach (var provider in providers)
            {
                // Add LLM provider specific validation
                switch (provider.ToLowerInvariant())
                {
                    case "openai":
                        if (options.LLM.OpenAI == null)
                        {
                            return ValidateOptionsResult.Fail($"OpenAI configuration is required when using OpenAI provider");
                        }
                        if (string.IsNullOrEmpty(options.LLM.OpenAI.ApiKey))
                        {
                            return ValidateOptionsResult.Fail("OpenAI ApiKey is required");
                        }
                        break;

                    case "claude":
                        if (options.LLM.Claude == null)
                        {
                            return ValidateOptionsResult.Fail($"Claude configuration is required when using Claude provider");
                        }
                        if (string.IsNullOrEmpty(options.LLM.Claude.ApiKey))
                        {
                            return ValidateOptionsResult.Fail("Claude ApiKey is required");
                        }
                        break;

                    case "deepseek":
                        if (options.LLM.DeepSeek == null)
                        {
                            return ValidateOptionsResult.Fail($"DeepSeek configuration is required when using DeepSeek provider");
                        }
                        if (string.IsNullOrEmpty(options.LLM.DeepSeek.ApiKey))
                        {
                            return ValidateOptionsResult.Fail("DeepSeek ApiKey is required");
                        }
                        break;

                    case "gemini":
                        if (options.LLM.Gemini == null)
                        {
                            return ValidateOptionsResult.Fail($"Gemini configuration is required when using Grok provider");
                        }
                        if (string.IsNullOrEmpty(options.LLM.Gemini.ApiKey))
                        {
                            return ValidateOptionsResult.Fail("Gemini ApiKey is required");
                        }
                        break;

                    default:
                        return ValidateOptionsResult.Fail($"Unsupported LLM provider: {provider}");
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}