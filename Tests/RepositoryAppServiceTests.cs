using Moq;
using AutoMapper;
using Xunit;
using BackendAPI.Application.DTOs;
using BackendAPI.Application.Interfaces;
using BackendAPI.Application.Services;
using BackendAPI.Domain.Interfaces;
using BackendAPI.Domain.Entities;
using BackendAPI.Application.DTOs.Repository;

namespace BackendAPI.Tests
{
    public class RepositoryAppServiceTests
    {
        private readonly Mock<IRepositoryService> _repositoryServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRelevanceCalculator> _relevanceCalculatorMock;
        private readonly RepositoryAppService _service;

        public RepositoryAppServiceTests()
        {
            _repositoryServiceMock = new Mock<IRepositoryService>();
            _mapperMock = new Mock<IMapper>();
            _relevanceCalculatorMock = new Mock<IRelevanceCalculator>();

            _service = new RepositoryAppService(
                _repositoryServiceMock.Object,
                _mapperMock.Object,
                _relevanceCalculatorMock.Object
            );
        }

        [Fact]
        public async Task SearchRepositoriesAsync_ShouldCalculateRelevanceForEachRepository()
        {
            var request = new SearchRequestDto { Query = "test", Page = 1, PerPage = 10 };
            var repositories = new List<Repository>
            {
                new Repository { Id = 1, Name = "test1" },
                new Repository { Id = 2, Name = "test2" }
            };

            var dtos = new List<RepositoryDto>
            {
                new RepositoryDto { Id = 1, Name = "test1" },
                new RepositoryDto { Id = 2, Name = "test2" }
            };

            _repositoryServiceMock.Setup(x => x.SearchRepositoriesAsync(request.Query, request.Page, request.PerPage))
                .ReturnsAsync(repositories);

            _mapperMock.Setup(x => x.Map<IEnumerable<RepositoryDto>>(repositories))
                .Returns(dtos);

            _relevanceCalculatorMock.Setup(x => x.CalculateScore(It.IsAny<RepositoryDto>()))
                .Returns(100);

            var result = await _service.SearchRepositoriesAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(100, r.RelevanceScore));
            _relevanceCalculatorMock.Verify(x => x.CalculateScore(It.IsAny<RepositoryDto>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetRelevantRepositoriesAsync_ShouldOrderByRelevanceScoreDescending()
        {
            var repositories = new List<Repository>
            {
                new Repository { Id = 1, Name = "repo1" },
                new Repository { Id = 2, Name = "repo2" }
            };

            var dtos = new List<RepositoryDto>
            {
                new RepositoryDto { Id = 1, Name = "repo1" },
                new RepositoryDto { Id = 2, Name = "repo2" }
            };

            _repositoryServiceMock.Setup(x => x.GetRelevantRepositories())
                .ReturnsAsync(repositories);

            _mapperMock.Setup(x => x.Map<IEnumerable<RepositoryDto>>(repositories))
                .Returns(dtos);

            _relevanceCalculatorMock.SetupSequence(x => x.CalculateScore(It.IsAny<RepositoryDto>()))
                .Returns(50)   // Primeiro repo
                .Returns(100); // Segundo repo

            var result = await _service.GetRelevantRepositoriesAsync();

            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList[0].Id); // Deve vir primeiro por ter score maior
            Assert.Equal(1, resultList[1].Id);
            Assert.True(resultList[0].RelevanceScore > resultList[1].RelevanceScore);
        }

        [Fact]
        public async Task ToggleFavoriteAsync_ShouldThrowArgumentException_WhenRepositoryNotFound()
        {
            var request = new ToggleFavoriteRequestDto { RepositoryId = 999 };

            _repositoryServiceMock.Setup(x => x.ToggleFavoriteAsync(request.RepositoryId))
                .ThrowsAsync(new ArgumentException("Repository not found"));


            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ToggleFavoriteAsync(request));
        }

        [Fact]
        public async Task ToggleFavoriteAsync_ShouldCalculateRelevance_AfterToggling()
        {
            var request = new ToggleFavoriteRequestDto { RepositoryId = 1 };
            var repository = new Repository { Id = 1, Name = "test", IsFavorite = true };
            var dto = new RepositoryDto { Id = 1, Name = "test", IsFavorite = true };

            _repositoryServiceMock.Setup(x => x.ToggleFavoriteAsync(request.RepositoryId))
                .ReturnsAsync(repository);

            _mapperMock.Setup(x => x.Map<RepositoryDto>(repository))
                .Returns(dto);

            _relevanceCalculatorMock.Setup(x => x.CalculateScore(dto))
                .Returns(150);

            var result = await _service.ToggleFavoriteAsync(request);

            Assert.NotNull(result);
            Assert.Equal(150, result.RelevanceScore);
            Assert.True(result.IsFavorite);
        }
    }
}