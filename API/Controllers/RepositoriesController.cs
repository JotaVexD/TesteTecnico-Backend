using Microsoft.AspNetCore.Mvc;
using BackendAPI.Application.Interfaces;
using BackendAPI.Application.DTOs.Repository;

namespace BackendAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RepositoriesController : ControllerBase
    {
        private readonly IRepositoryAppService _repositoryAppService;
        private readonly ILogger<RepositoriesController> _logger;

        public RepositoriesController(
            IRepositoryAppService repositoryAppService,
            ILogger<RepositoriesController> logger)
        {
            _repositoryAppService = repositoryAppService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                    return BadRequest("Query parameter is required");

                var repositories = await _repositoryAppService.SearchRepositoriesAsync(request);
                return Ok(repositories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching repositories");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequestDto request)
        {
            try
            {
                var repository = await _repositoryAppService.ToggleFavoriteAsync(request);
                return Ok(repository);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Relevant()
        {
            try
            {
                var repositories = await _repositoryAppService.GetRelevantRepositoriesAsync();
                return Ok(repositories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting relevant repositories");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}