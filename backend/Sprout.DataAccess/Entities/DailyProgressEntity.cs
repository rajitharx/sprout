namespace Sprout.DataAccess.Entities;

public class DailyProgressEntity
{
    public int Id { get; set; }
    public string Date { get; set; } = "";
    public List<CompletedTaskEntity> CompletedTasks { get; set; } = [];
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
