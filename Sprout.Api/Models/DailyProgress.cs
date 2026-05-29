namespace Sprout.Api.Models;

public record DailyProgress
{
    public string Date { get; init; } = "";
    public List<string> CompletedTaskIds { get; init; } = [];
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}
