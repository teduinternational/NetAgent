namespace NetAgent.Abstractions.LLM
{
    public interface ILLMPreferences
    {
        IEnumerable<string> PreferredProviders { get; }
        double GetProviderWeight(string providerName);
        bool IsProviderAllowed(string providerName);
    }
}