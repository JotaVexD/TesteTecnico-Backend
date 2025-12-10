using BackendAPI.Application.DTOs.Repository;

namespace BackendAPI.Application.Interfaces
{
    public interface IRelevanceCalculator
    {
        int CalculateScore(RepositoryDto repository);
    }
}