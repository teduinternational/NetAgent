using NetAgent.Core.Memory;
using NetAgent.Memory.InMemory;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NetAgent.Core.Tests.Memory
{
    public class InMemoryMemoryStoreTests
    {
        private readonly InMemoryMemoryStore _store;

        public InMemoryMemoryStoreTests()
        {
            _store = new InMemoryMemoryStore();
        }

        [Fact]
        public async Task SaveAsync_ShouldStoreValue()
        {
            // Arrange
            var goal = "test goal";
            var response = "test response";

            // Act
            await _store.SaveAsync(goal, response);
            var result = await _store.RetrieveAsync(goal);

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task RetrieveAsync_WhenKeyDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _store.RetrieveAsync("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveAsync_WithNullGoal_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _store.SaveAsync(null!, "response"));
        }

        [Fact]
        public async Task SaveAsync_WithNullResponse_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _store.SaveAsync("goal", null!));
        }

        [Fact]
        public async Task SaveAsync_ShouldUpdateExistingValue()
        {
            // Arrange
            var goal = "test goal";
            var response1 = "test response 1";
            var response2 = "test response 2";

            // Act
            await _store.SaveAsync(goal, response1);
            await _store.SaveAsync(goal, response2);
            var result = await _store.RetrieveAsync(goal);

            // Assert
            Assert.Equal(response2, result);
        }
    }
}