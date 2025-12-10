using BackendAPI.Application.DTOs.Repository;

namespace BackendAPI.Application.Interfaces
{
    public interface IRepositoryAppService
    {
        Task<SearchResultDto> SearchRepositoriesAsync(SearchRequestDto request);
        Task<RepositoryDto> ToggleFavoriteAsync(ToggleFavoriteRequestDto request);
        Task<IEnumerable<RepositoryDto>> GetRelevantRepositoriesAsync();
    }
}