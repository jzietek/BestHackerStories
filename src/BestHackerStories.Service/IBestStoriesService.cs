using BestHackerStories.Shared.DataTransferObjects;

namespace BestHackerStories.Service;

public interface IBestStoriesService
{
    Task<IEnumerable<StoryDto>> GetBestStories(int maxItems);
}