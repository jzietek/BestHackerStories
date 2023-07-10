namespace BestHackerStories.Shared.DataTransferObjects;

public record StoryDto(string Title, string Uri, string PostedBy, DateTimeOffset Time, int Score, int CommentCount);