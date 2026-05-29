namespace Sprout.Api.Models;

public record HabitTask
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Label { get; init; } = "";
    public string Emoji { get; init; } = "🪥";
    public int SortOrder { get; init; } = 0;
    public bool IsActive { get; init; } = true;
}
