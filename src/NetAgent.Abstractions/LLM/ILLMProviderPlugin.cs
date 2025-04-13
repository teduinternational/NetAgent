using Microsoft.Extensions.DependencyInjection;
using NetAgent.Abstractions.LLM;

namespace NetAgent.Abstractions.LLM
{
    public interface ILLMProviderPlugin
    {
        /// <summary>
        /// Gets the name of the LLM provider plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates an instance of the LLM provider
        /// </summary>
        /// <param name="services">The service provider for dependency injection</param>
        /// <returns>An instance of ILLMProvider</returns>
        ILLMProvider CreateProvider(IServiceProvider services);
        
        /// <summary>
        /// Gets the configuration type for this provider
        /// </summary>
        Type ConfigurationType { get; }
    }
}