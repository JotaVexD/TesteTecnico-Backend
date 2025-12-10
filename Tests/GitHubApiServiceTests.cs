using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Text.Json;
using BackendAPI.Domain.Entities;
using BackendAPI.Infrastructure.Services;

namespace BackendAPI.Tests
{
    public class GitHubApiServiceTests
    {
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly Mock<ILogger<GitHubApiService>> _loggerMock;
        private readonly IMemoryCache _cache;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly GitHubApiService _service;

        public GitHubApiServiceTests()
        {
            _httpClientMock = new Mock<HttpClient>();
            _loggerMock = new Mock<ILogger<GitHubApiService>>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _configurationMock = new Mock<IConfiguration>();

            _service = new GitHubApiService(
                _httpClientMock.Object,
                _cache,
                _configurationMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public void ToggleFavoriteAsync_ShouldAddToFavorites_WhenNotFavorite()
        {
        }

        [Fact]
        public void MapToRepository_ShouldParseJsonCorrectly()
        {
            var json = @"{
                ""id"": 123,
                ""name"": ""test-repo"",
                ""full_name"": ""owner/test-repo"",
                ""description"": ""Test repository"",
                ""html_url"": ""https://github.com/owner/test-repo"",
                ""language"": ""C#"",
                ""stargazers_count"": 100,
                ""forks_count"": 50,
                ""watchers_count"": 80,
                ""updated_at"": ""2024-01-01T10:00:00Z""
            }";

            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

        }
    }
}