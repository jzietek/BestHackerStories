using System.Net.Http.Json;
using BestHackerStories.Api.Controllers;
using BestHackerStories.Shared.DataTransferObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BestHackerStories.Api.IntegrationTest;

public class Tests
{
    private WebApplicationFactory<BestStoriesController> _application;
    private HttpClient _client;

    [OneTimeSetUp]
    public void Setup()
    {
        _application = new WebApplicationFactory<BestStoriesController>();
        _client = _application.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _application.Dispose();
    }

    [Test]
    public async Task SucceedsWithDefaultItemsCount_IfNoLimitIsProvided()
    {
        var response = await _client.GetAsync("/api/beststories");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var stories = await response.Content.ReadFromJsonAsync<IEnumerable<StoryDto>>();
        stories.Should().NotBeNull();
        stories!.Count().Should().Be(10);
    }

    [Test]
    public async Task SucceedsWithLimitedItemsCount_IfLimitIsProvided()
    {
        var response = await _client.GetAsync("/api/beststories?maxItems=2");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var stories = await response.Content.ReadFromJsonAsync<IEnumerable<StoryDto>>();
        stories.Should().NotBeNull();
        stories!.Count().Should().Be(2);
    }
}
