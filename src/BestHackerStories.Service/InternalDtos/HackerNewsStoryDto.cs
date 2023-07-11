namespace BestHackerStories.Service.InternalDtos;

public record HackerNewsStoryDto(
        string By,
        int Descendants,
        int Id,
        int[] Kids,
        int Score,
        long Time, //Creation time, Linux Epoch date UTC
        string Title,
        string Type,
        string Url
        );