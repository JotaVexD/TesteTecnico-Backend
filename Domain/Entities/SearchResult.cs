using BackendAPI.Domain.Entities;

public class SearchResult
{
    public int TotalCount { get; set; }
    public List<Repository> Items { get; set; } = new();
}