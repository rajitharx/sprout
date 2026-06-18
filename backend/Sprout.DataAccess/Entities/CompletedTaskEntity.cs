namespace Sprout.DataAccess.Entities;

public class CompletedTaskEntity
{
    public int Id { get; set; }
    public int DailyProgressId { get; set; }
    public string TaskId { get; set; } = "";
    public DailyProgressEntity? DailyProgress { get; set; }
}
