using System.Collections.Concurrent;
using System.Net.Http.Json;
using BestHackerStories.Service.InternalDtos;
using BestHackerStories.Shared.DataTransferObjects;
using Microsoft.Extensions.Configuration;

namespace BestHackerStories.Service;

public sealed class BestStoriesService : IBestStoriesService, IDisposable
{
    private string _bestStoriesUrl;
    private string _storyUrlFormat;
    private readonly int _maxCrawlerThreads;
    private readonly HttpClient _httpClient;

    public BestStoriesService(IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        _bestStoriesUrl = configuration["HackerNews:BestStoriesUrl"] ?? throw new ArgumentNullException("HackerNews:BestStoriesUrl");
        _storyUrlFormat = configuration["HackerNews:StoryUrlFormat"] ?? throw new ArgumentNullException("HackerNews:StoryUrlFormat");
        _maxCrawlerThreads = int.Parse(configuration["HackerNews:MaxCrawlerThreads"] ?? "MISSING");
        _httpClient = clientFactory.CreateClient();
    }

    public async Task<IEnumerable<StoryDto>> GetBestStories(int? maxItems)
    {
        var bestStoriesIds = await GetBestStoriesIds();

        using (var semaphore = new SemaphoreSlim(initialCount: _maxCrawlerThreads, maxCount: _maxCrawlerThreads))
        {
            var responses = new ConcurrentBag<StoryDto>();
            var tasks = bestStoriesIds.Select(async id =>
            {
                await semaphore.WaitAsync();
                try
                {
                    HackerNewsStoryDto internalStory = await GetHackerNewsStory(id);
                    StoryDto mappedStory = MapStory(internalStory);
                    responses.Add(mappedStory);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            return responses.OrderByDescending(story => story.Score)
                .Take(maxItems ?? int.MaxValue).ToArray();
        }
    }

    private async Task<HackerNewsStoryDto> GetHackerNewsStory(int id)
    {
        var uri = string.Format(_storyUrlFormat, id);
        var response = await _httpClient.GetAsync(uri);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<HackerNewsStoryDto>();
    }

    private StoryDto MapStory(HackerNewsStoryDto input)
    {
        return new StoryDto(        
            Title: input.Title,
            Uri: input.Url,
            PostedBy: input.By,
            Time: DateTimeOffset.FromUnixTimeSeconds(input.Time).ToString("yyyy-MM-ddTHH:mm:sszzz"),
            Score: input.Score,
            CommentCount: input.Kids.Count()
        );
    }

    private async Task<IEnumerable<int>> GetBestStoriesIds()
    {
        var response = await _httpClient.GetAsync(_bestStoriesUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int[]>() ?? Enumerable.Empty<int>();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
