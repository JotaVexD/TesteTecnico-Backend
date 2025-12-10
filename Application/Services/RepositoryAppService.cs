using AutoMapper;
using BackendAPI.Application.DTOs.Repository;
using BackendAPI.Application.Interfaces;
using BackendAPI.Domain.Interfaces;

namespace BackendAPI.Application.Services
{
    public class RepositoryAppService : IRepositoryAppService
    {
        private readonly IRepositoryService _repositoryService;
        private readonly IMapper _mapper;
        private readonly IRelevanceCalculator _relevanceCalculator;

        public RepositoryAppService(
            IRepositoryService repositoryService,
            IMapper mapper,
            IRelevanceCalculator relevanceCalculator)
        {
            _repositoryService = repositoryService;
            _mapper = mapper;
            _relevanceCalculator = relevanceCalculator;
        }

        public async Task<SearchResultDto> SearchRepositoriesAsync(SearchRequestDto request)
        {
            var searchResult = await _repositoryService.SearchRepositoriesAsync(request.Query, request.Page, request.PerPage);
            var dtos = _mapper.Map<IEnumerable<RepositoryDto>>(searchResult.Items);

            foreach (var dto in dtos)
            {
                dto.RelevanceScore = _relevanceCalculator.CalculateScore(dto);
            }

            return new SearchResultDto
            {
                TotalCount = searchResult.TotalCount,
                Items = dtos
            };
        }

        public async Task<RepositoryDto> ToggleFavoriteAsync(ToggleFavoriteRequestDto request)
        {
            var repository = await _repositoryService.ToggleFavoriteAsync(request.RepositoryId);
            var dto = _mapper.Map<RepositoryDto>(repository);

            // Calcula o score após favoritar/desfavoritar
            dto.RelevanceScore = _relevanceCalculator.CalculateScore(dto);

            return dto;
        }

        public async Task<IEnumerable<RepositoryDto>> GetRelevantRepositoriesAsync()
        {
            var repositories = await _repositoryService.GetRelevantRepositories();
            var dtos = _mapper.Map<IEnumerable<RepositoryDto>>(repositories);

            foreach (var dto in dtos)
            {
                dto.RelevanceScore = _relevanceCalculator.CalculateScore(dto);
            }
            return dtos.OrderByDescending(r => r.RelevanceScore);
        }
    }
}