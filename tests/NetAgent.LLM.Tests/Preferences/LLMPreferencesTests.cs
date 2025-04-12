using NetAgent.LLM.Preferences;

namespace NetAgent.LLM.Tests.Preferences
{
    public class LLMPreferencesTests
    {
        [Fact]
        public void Constructor_WithNullPreferredProviders_ShouldInitializeEmptyList()
        {
            // Act
            var preferences = new LLMPreferences(preferredProviders: null);

            // Assert
            Assert.Empty(preferences.PreferredProviders);
        }

        [Fact]
        public void Constructor_WithPreferredProviders_ShouldInitializeList()
        {
            // Arrange
            var providers = new[] { "Provider1", "Provider2", "Provider3" };

            // Act
            var preferences = new LLMPreferences(providers);

            // Assert
            Assert.Equal(providers, preferences.PreferredProviders);
        }

        [Fact]
        public void GetProviderWeight_ShouldReturnDecreasingWeights()
        {
            // Arrange
            var providers = new[] { "Provider1", "Provider2", "Provider3" };
            var preferences = new LLMPreferences(providers);

            // Act & Assert
            var weight1 = preferences.GetProviderWeight("Provider1");
            var weight2 = preferences.GetProviderWeight("Provider2");
            var weight3 = preferences.GetProviderWeight("Provider3");

            Assert.True(weight1 > weight2);
            Assert.True(weight2 > weight3);
        }

        [Fact]
        public void GetProviderWeight_ForUnknownProvider_ShouldReturnZero()
        {
            // Arrange
            var providers = new[] { "Provider1", "Provider2" };
            var preferences = new LLMPreferences(providers);

            // Act
            var weight = preferences.GetProviderWeight("UnknownProvider");

            // Assert
            Assert.Equal(0.0, weight);
        }

        [Fact]
        public void IsProviderAllowed_WithNullPreferences_ShouldAllowAllProviders()
        {
            // Arrange
            var preferences = new LLMPreferences(preferredProviders: null);

            // Act & Assert
            Assert.True(preferences.IsProviderAllowed("AnyProvider"));
        }

        [Fact]
        public void IsProviderAllowed_WithEmptyPreferences_ShouldAllowAllProviders()
        {
            // Arrange
            var preferences = new LLMPreferences(Array.Empty<string>());

            // Act & Assert
            Assert.True(preferences.IsProviderAllowed("AnyProvider"));
        }

        [Fact]
        public void IsProviderAllowed_WithPreferences_ShouldOnlyAllowListedProviders()
        {
            // Arrange
            var providers = new[] { "Provider1", "Provider2" };
            var preferences = new LLMPreferences(providers);

            // Act & Assert
            Assert.True(preferences.IsProviderAllowed("Provider1"));
            Assert.True(preferences.IsProviderAllowed("Provider2"));
            Assert.False(preferences.IsProviderAllowed("Provider3"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void IsProviderAllowed_WithInvalidProviderName_ShouldReturnFalseWhenPreferencesExist(string providerName)
        {
            // Arrange
            var providers = new[] { "Provider1", "Provider2" };
            var preferences = new LLMPreferences(providers);

            // Act & Assert
            Assert.False(preferences.IsProviderAllowed(providerName));
        }
    }
}