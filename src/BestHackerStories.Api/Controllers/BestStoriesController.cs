using BestHackerStories.Service;
using BestHackerStories.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace BestHackerStories.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BestStoriesController : ControllerBase
    {
        private readonly ILogger<BestStoriesController> _logger;
        private readonly IBestStoriesService _bestStoriesService;

        public BestStoriesController(ILogger<BestStoriesController> logger, IBestStoriesService bestStoriesService)
        {
            _logger = logger;
            _bestStoriesService = bestStoriesService;
        }

        [HttpGet(Name = "GetBestStories")]
        public async Task<IActionResult> GetBestStories(int? maxItems, CancellationToken cancellationToken)
        {
            IEnumerable<StoryDto> results = await _bestStoriesService.GetBestStories(maxItems, cancellationToken);
            return Ok(results);
        }
    }
}
