namespace BackendAPI.Application.DTOs.Repository
{
    public class SearchResultDto
    {
        public int TotalCount { get; set; }
        public IEnumerable<RepositoryDto> Items { get; set; } = new List<RepositoryDto>();
    }
}