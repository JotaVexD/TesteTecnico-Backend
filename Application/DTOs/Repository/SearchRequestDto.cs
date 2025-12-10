namespace BackendAPI.Application.DTOs.Repository
{
    public class SearchRequestDto
    {
        public string Query { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PerPage { get; set; } = 100;
    }
}