using BackendAPI.Domain.Entities;

namespace BackendAPI.Domain.Interfaces
{
    public interface IRepositoryService
    {
        Task<IEnumerable<Repository>> SearchRepositoriesAsync(string query, int page, int perPage);
        Task<Repository> ToggleFavoriteAsync(int repositoryId);
        Task<IEnumerable<Repository>> GetRelevantRepositories();
        Task<Repository?> GetRepositoryAsync(int repositoryId);
    }
}