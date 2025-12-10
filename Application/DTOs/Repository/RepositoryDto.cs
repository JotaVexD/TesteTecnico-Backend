namespace BackendAPI.Application.DTOs.Repository
{
    public class RepositoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int StargazersCount { get; set; }
        public int ForksCount { get; set; }
        public int WatchersCount { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool IsFavorite { get; set; }
        public int RelevanceScore { get; set; }
    }
}