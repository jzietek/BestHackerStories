using System.Net.Http.Json;
using BestHackerStories.Service.InternalDtos;
using BestHackerStories.Shared.DataTransferObjects;
using Microsoft.Extensions.Configuration;

namespace BestHackerStories.Service;

public sealed class BestStoriesService : IBestStoriesService, IDisposable
{
    private string _bestStoriesUrl;
    private string _storyUrlFormat;
    private readonly HttpClient _httpClient;

    public BestStoriesService(IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        _bestStoriesUrl = configuration["HackerNews:BestStoriesUrl"] ?? throw new ArgumentNullException("HackerNews:BestStoriesUrl");
        _storyUrlFormat = configuration["HackerNews:StoryUrlFormat"] ?? throw new ArgumentNullException("HackerNews:StoryUrlFormat");

        _httpClient = clientFactory.CreateClient();
        
    }

    public async Task<IEnumerable<StoryDto>> GetBestStories(int? maxItems)
    {
        //var results = Enumerable.Range(1, 20)
        //    .Select(i => new StoryDto($"Title {i}", $"Uri {i}", $"Author {i}", DateTimeOffset.Now.AddDays(-i), 100 - i, 10 + i ))
        //    .Take(maxItems ?? int.MaxValue)
        //    .ToList();

        var bestStoriesIds = await GetBestStoriesIds();
        if (maxItems.HasValue)
        {
            bestStoriesIds = bestStoriesIds.Take(maxItems.Value);
        }

        var results = new List<StoryDto>();
        foreach(var id in bestStoriesIds)
        {
            HackerNewsStoryDto internalStory = await GetHackerNewsStory(id);
            StoryDto mappedStory = MapStory(internalStory);
            results.Add(mappedStory);
        }

        return results;
    }

    private async Task<HackerNewsStoryDto> GetHackerNewsStory(int id)
    {
        var uri = string.Format(_storyUrlFormat, id);
        var response = await _httpClient.GetAsync(uri);

        //TODO missing results exception handling

        return await response.Content.ReadFromJsonAsync<HackerNewsStoryDto>();
    }

    private StoryDto MapStory(HackerNewsStoryDto input)
    {
        return new StoryDto(        
            Title: input.Title,
            Uri: input.Url,
            PostedBy: input.By,
            Time: DateTimeOffset.FromUnixTimeSeconds(input.Time),
            Score: input.Score,
            CommentCount: input.Kids.Count()
        );
    }

    private async Task<IEnumerable<int>> GetBestStoriesIds()
    {
        var response = await _httpClient.GetAsync(_bestStoriesUrl);
        return await response.Content.ReadFromJsonAsync<int[]>() ?? Enumerable.Empty<int>();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

