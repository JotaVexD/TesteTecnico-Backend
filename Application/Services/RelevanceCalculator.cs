using BackendAPI.Application.DTOs.Repository;
using BackendAPI.Application.Interfaces;

public class RelevanceCalculator : IRelevanceCalculator
{
    /// <summary>
    /// Calcula o score de relevância com pesos balanceados.
    /// Lógica: Stars (peso 3), Forks (peso 2), Watchers (peso 1)
    /// Recência: bônus decrescente até 30 dias (máx 30 pontos)
    /// Favorito: bônus moderado de 50 pontos para destacar, mas não distorcer
    /// </summary>
    public int CalculateScore(RepositoryDto repository)
    {
        const int STAR_WEIGHT = 3;
        const int FORK_WEIGHT = 2;
        const int WATCHER_WEIGHT = 1;
        const int RECENCY_BONUS_MAX_DAYS = 30;
        const int FAVORITE_BONUS = 50;

        long baseScore = (long)repository.StargazersCount * STAR_WEIGHT
                       + (long)repository.ForksCount * FORK_WEIGHT
                       + (long)repository.WatchersCount * WATCHER_WEIGHT;

        // Bônus por recência (0-30 pontos)
        var daysSinceUpdate = (DateTimeOffset.UtcNow - repository.UpdatedAt).Days;

        // Se a data for futura, considerar como 0 dias (atualizado agora)
        if (daysSinceUpdate < 0)
        {
            daysSinceUpdate = 0;
        }

        if (daysSinceUpdate <= RECENCY_BONUS_MAX_DAYS)
        {
            int recencyBonus = RECENCY_BONUS_MAX_DAYS - daysSinceUpdate;
            baseScore += recencyBonus;
        }

        if (repository.IsFavorite)
        {
            baseScore += FAVORITE_BONUS;
        }

        if (baseScore > int.MaxValue)
        {
            return int.MaxValue;
        }

        if (baseScore < int.MinValue)
        {
            return int.MinValue;
        }

        return (int)baseScore;
    }
}