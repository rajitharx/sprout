namespace Sprout.DataAccess.Entities;

public class HabitTaskEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = "";
    public string Emoji { get; set; } = "🪥";
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
