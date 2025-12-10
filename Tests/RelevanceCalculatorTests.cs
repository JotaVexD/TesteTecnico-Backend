using Xunit;
using BackendAPI.Application.DTOs.Repository;
using BackendAPI.Application.Services;
using System;

namespace BackendAPI.Tests
{
    public class RelevanceCalculatorTests
    {
        private readonly RelevanceCalculator _calculator;

        public RelevanceCalculatorTests()
        {
            _calculator = new RelevanceCalculator();
        }

        [Fact]
        public void CalculateScore_ShouldReturnZero_WhenRepositoryHasNoMetrics()
        {
            var repo = new RepositoryDto
            {
                StargazersCount = 0,
                ForksCount = 0,
                WatchersCount = 0,
                IsFavorite = false,
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-40)
            };

            var score = _calculator.CalculateScore(repo);
            Assert.Equal(0, score);
        }

        [Fact]
        public void CalculateScore_ShouldApplyCorrectWeights()
        {
            var repo = new RepositoryDto
            {
                StargazersCount = 10,    // 10 * 3 = 30
                ForksCount = 5,          // 5 * 2 = 10
                WatchersCount = 3,       // 3 * 1 = 3
                IsFavorite = false,
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10) // Recency: 30 - 10 = 20
            };

            var score = _calculator.CalculateScore(repo);
            // 30 (stars) + 10 (forks) + 3 (watchers) + 20 (recency) = 63
            Assert.Equal(63, score);
        }

        [Theory]
        [InlineData(0, 30)]   // Hoje: bônus máximo
        [InlineData(15, 15)]  // 15 dias: bônus 15
        [InlineData(30, 0)]   // 30 dias: bônus 0
        [InlineData(31, 0)]   // 31 dias: sem bônus
        public void CalculateScore_ShouldApplyRecencyBonusCorrectly(int daysAgo, int expectedBonus)
        {
            var repo = new RepositoryDto
            {
                StargazersCount = 0,
                ForksCount = 0,
                WatchersCount = 0,
                IsFavorite = false,
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-daysAgo)
            };

            var score = _calculator.CalculateScore(repo);
            Assert.Equal(expectedBonus, score);
        }

        [Fact]
        public void CalculateScore_ShouldAddFavoriteBonus_WhenRepositoryIsFavorite()
        {
            var repo = new RepositoryDto
            {
                StargazersCount = 10,
                ForksCount = 5,
                WatchersCount = 3,
                IsFavorite = true,
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            };

            var score = _calculator.CalculateScore(repo);
            // 30 (stars) + 10 (forks) + 3 (watchers) + 20 (recency) + 50 (favorite) = 113
            Assert.Equal(113, score);
        }

        [Fact]
        public void CalculateScore_ShouldPrioritizePopularRepositories_OverFavorites()
        {
            var popularRepo = new RepositoryDto
            {
                StargazersCount = 1000,  // 1000 * 3 = 3000
                ForksCount = 500,        // 500 * 2 = 1000
                WatchersCount = 300,     // 300 * 1 = 300
                IsFavorite = false,
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10) // Recency: 30 - 10 = 20
            };
            // Total: 3000 + 1000 + 300 + 20 = 4320

            var favoriteRepo = new RepositoryDto
            {
                StargazersCount = 10,    // 10 * 3 = 30
                ForksCount = 5,          // 5 * 2 = 10
                WatchersCount = 3,       // 3 * 1 = 3
                IsFavorite = true,
                UpdatedAt = DateTimeOffset.UtcNow // Recency: 30 - 0 = 30
            };
            // Total: 30 + 10 + 3 + 30 + 50 = 123

            var popularScore = _calculator.CalculateScore(popularRepo);
            var favoriteScore = _calculator.CalculateScore(favoriteRepo);

            Assert.True(popularScore > favoriteScore, "Repositório popular deve ter score maior mesmo sem ser favorito");
        }

        [Fact]
        public void CalculateScore_ShouldHandleFutureDates_WithMaxRecencyBonus()
        {
            var repo = new RepositoryDto
            {
                StargazersCount = 0,
                ForksCount = 0,
                WatchersCount = 0,
                IsFavorite = false,
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(5) // Data futura, deve ser tratada como hoje
            };

            var score = _calculator.CalculateScore(repo);
            // Data futura tratada como hoje: recency bonus máximo (30)
            Assert.Equal(30, score);
        }

        [Fact]
        public void CalculateScore_ShouldHandleEdgeCases()
        {
            // Teste com valores grandes mas seguros (evitar overflow)
            var repo = new RepositoryDto
            {
                StargazersCount = 1000000,
                ForksCount = 50000,
                WatchersCount = 20000,
                IsFavorite = true,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            var score = _calculator.CalculateScore(repo);

            // Score deve ser positivo (não pode ser negativo devido a overflow)
            Assert.True(score > 0, $"Score deveria ser positivo mas é {score}");
        }

        [Fact]
        public void CalculateScore_ShouldReturnCorrectScore_WhenOnlyFavorite()
        {
            var repo = new RepositoryDto
            {
                StargazersCount = 0,
                ForksCount = 0,
                WatchersCount = 0,
                IsFavorite = true,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            var score = _calculator.CalculateScore(repo);
            // 0 (base) + 30 (recency) + 50 (favorite) = 80
            Assert.Equal(80, score);
        }
    }
}