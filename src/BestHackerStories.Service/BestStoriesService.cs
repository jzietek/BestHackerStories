using BestHackerStories.Shared.DataTransferObjects;

namespace BestHackerStories.Service;

public class BestStoriesService : IBestStoriesService
{
    public async Task<IEnumerable<StoryDto>> GetBestStories(int maxItems)
    {
        var results = Enumerable.Range(1, 20)
            .Select(i => new StoryDto($"Title {i}", $"Uri {i}", $"Author {i}", DateTimeOffset.Now.AddDays(-i), 100 - i, 10 + i ))
            .Take(maxItems)
            .ToList();

        return await Task.FromResult(results);
    }
}

