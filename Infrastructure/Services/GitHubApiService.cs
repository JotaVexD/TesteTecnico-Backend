using BackendAPI.Domain.Entities;
using BackendAPI.Domain.Interfaces;

namespace BackendAPI.Infrastructure.Services
{
    public class GitHubApiService : IRepositoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GitHubApiService> _logger;

        private static readonly Dictionary<int, Repository> _favorites = new();
        private static readonly object _favoritesLock = new();
        private static readonly List<Repository> _searchedRepositories = new();

        public GitHubApiService(
            HttpClient httpClient,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<GitHubApiService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;

            _httpClient.BaseAddress = new Uri("https://api.github.com/");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GitHubSearchApp");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");

            var githubToken = _configuration["GitHub:Token"];
            if (!string.IsNullOrEmpty(githubToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", githubToken);
            }
        }

        public async Task<IEnumerable<Repository>> SearchRepositoriesAsync(string query,int page,int perPage)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<Repository>();

            var cacheKey = $"search_{query.ToLower()}";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Repository>? repositories))
            {
                try
                {
                    var response = await _httpClient.GetAsync(
                        $"search/repositories?q={Uri.EscapeDataString(query)}&per_page={perPage}&page={page}&sort=stars&order=desc");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    repositories = doc.RootElement
                        .GetProperty("items")
                        .EnumerateArray()
                        .Select(item => MapToRepository(item))
                        .ToList();

                    _cache.Set(cacheKey, repositories, TimeSpan.FromMinutes(5));

                    lock (_favoritesLock)
                    {
                        foreach (var repo in repositories)
                        {
                            if (!_searchedRepositories.Any(r => r.Id == repo.Id))
                                _searchedRepositories.Add(repo);
                        }
                    }

                    _logger.LogInformation("Found {Count} repositories for query: {Query}",
                        repositories.Count(), query);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching repositories for query: {Query}", query);
                    throw;
                }
            }

            return repositories ?? Enumerable.Empty<Repository>();
        }

        public async Task<Repository> ToggleFavoriteAsync(int repositoryId)
        {
            var repository = await GetRepositoryAsync(repositoryId);
            if (repository == null)
                throw new ArgumentException($"Repository with ID {repositoryId} not found");

            lock (_favoritesLock)
            {
                if (_favorites.ContainsKey(repositoryId))
                {
                    _favorites.Remove(repositoryId);
                    repository.IsFavorite = false;
                    repository.FavoritedAt = null;
                    _logger.LogInformation("Repository {Id} removed from favorites", repositoryId);
                }
                else
                {
                    repository.IsFavorite = true;
                    repository.FavoritedAt = DateTime.UtcNow;
                    _favorites[repositoryId] = repository;
                    _logger.LogInformation("Repository {Id} added to favorites", repositoryId);
                }
            }

            return repository;
        }

        public Task<IEnumerable<Repository>> GetRelevantRepositories()
        {
            var allRepositories = new List<Repository>();

            lock (_favoritesLock)
            {
                allRepositories.AddRange(_searchedRepositories);
            }

            lock (_favoritesLock)
            {
                allRepositories.AddRange(_favorites.Values);
            }

            var distinctRepositories = allRepositories
                .GroupBy(r => r.Id)
                .Select(g => g.OrderByDescending(r => r.FavoritedAt ?? DateTime.MinValue).First())
                .ToList();

            return Task.FromResult<IEnumerable<Repository>>(distinctRepositories);
        }

        public async Task<Repository?> GetRepositoryAsync(int repositoryId)
        {
            lock (_favoritesLock)
            {
                if (_favorites.TryGetValue(repositoryId, out var favorite))
                    return favorite;
            }

            try
            {
                var response = await _httpClient.GetAsync($"repositories/{repositoryId}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                return MapToRepository(doc.RootElement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repository by ID: {RepositoryId}", repositoryId);
                return null;
            }
        }

        private Repository MapToRepository(JsonElement element)
        {
            var id = element.GetProperty("id").GetInt32();

            bool isFavorite;
            DateTime? favoritedAt = null;

            lock (_favoritesLock)
            {
                isFavorite = _favorites.ContainsKey(id);
                if (isFavorite && _favorites.TryGetValue(id, out var fav))
                    favoritedAt = fav.FavoritedAt;
            }

            return new Repository
            {
                Id = id,
                Name = element.GetProperty("name").GetString() ?? string.Empty,
                FullName = element.GetProperty("full_name").GetString() ?? string.Empty,
                Description = element.TryGetProperty("description", out var desc) ?
                    desc.GetString() ?? string.Empty : string.Empty,
                HtmlUrl = element.GetProperty("html_url").GetString() ?? string.Empty,
                Language = element.TryGetProperty("language", out var lang) ?
                    lang.GetString() ?? string.Empty : string.Empty,
                StargazersCount = element.GetProperty("stargazers_count").GetInt32(),
                ForksCount = element.GetProperty("forks_count").GetInt32(),
                WatchersCount = element.GetProperty("watchers_count").GetInt32(),
                UpdatedAt = element.GetProperty("updated_at").GetDateTimeOffset(),
                IsFavorite = isFavorite,
                FavoritedAt = favoritedAt
            };
        }
    }
}