using NetAgent.Abstractions.LLM;

namespace NetAgent.LLM.Preferences
{
    public class LLMPreferences : ILLMPreferences
    {
        private readonly Dictionary<string, double> _providerWeights;
        private readonly HashSet<string> _allowedProviders;

        public LLMPreferences(IEnumerable<string> preferredProviders)
        {
            PreferredProviders = preferredProviders?.ToList() ?? new List<string>();
            _providerWeights = new Dictionary<string, double>();
            _allowedProviders = new HashSet<string>(preferredProviders ?? Enumerable.Empty<string>());

            // Initialize weights based on order of preference
            var weight = 1.0;
            foreach (var provider in PreferredProviders)
            {
                _providerWeights[provider] = weight;
                weight *= 0.8; // Decrease weight for each subsequent provider
            }
        }

        public IEnumerable<string> PreferredProviders { get; }

        public double GetProviderWeight(string providerName)
        {
            return _providerWeights.TryGetValue(providerName, out var weight) ? weight : 0.0;
        }

        public bool IsProviderAllowed(string providerName)
        {
            return _allowedProviders.Count == 0 || _allowedProviders.Contains(providerName);
        }
    }
}