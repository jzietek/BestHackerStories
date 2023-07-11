using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Threading;
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

        TimeSpan crawlerTimeout = TimeSpan.Parse(configuration["HackerNews:CrawlerTimeout"] ?? "MISSING");
        _httpClient = clientFactory.CreateClient();
        _httpClient.Timeout = crawlerTimeout;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    public async Task<IEnumerable<StoryDto>> GetBestStories(int? maxItems, CancellationToken cancellationToken)
    {
        var bestStoriesIds = await GetBestStoriesIds(cancellationToken);

        using (var semaphore = new SemaphoreSlim(initialCount: _maxCrawlerThreads, maxCount: _maxCrawlerThreads))
        {
            var responses = new ConcurrentBag<StoryDto>();
            var tasks = bestStoriesIds.Select(async id =>
            {
                await semaphore.WaitAsync();
                try
                {
                    HackerNewsStoryDto? internalStory = await GetHackerNewsStory(id, cancellationToken);
                    if (internalStory is null)
                        return;

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

    private async Task<HackerNewsStoryDto?> GetHackerNewsStory(int id, CancellationToken cancellationToken)
    {
        var uri = string.Format(_storyUrlFormat, id);
        var response = await _httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<HackerNewsStoryDto>();
    }

    private StoryDto MapStory(HackerNewsStoryDto input)
    {
        string time = DateTimeOffset.FromUnixTimeSeconds(input.Time).ToString("yyyy-MM-ddTHH:mm:sszzz");
        return new StoryDto(        
            Title: input.Title,
            Uri: input.Url,
            PostedBy: input.By,
            Time: time,
            Score: input.Score,
            CommentCount: input.Kids.Count()
        );
    }

    private async Task<IEnumerable<int>> GetBestStoriesIds(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(_bestStoriesUrl, cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int[]>() ?? Enumerable.Empty<int>();
    }    
}
