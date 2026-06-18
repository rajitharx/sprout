using Sprout.DataAccess.Entities;

namespace Sprout.DataAccess.Repositories;

public interface IProgressRepository
{
    Task<DailyProgressEntity?> GetByDateAsync(string date);
    Task<List<DailyProgressEntity>> GetWeekAsync(string startDate, string endDate);
    Task<DailyProgressEntity> CreateOrUpdateAsync(string date, List<string> completedTaskIds);
    Task<DailyProgressEntity> MarkCompleteAsync(string date, string taskId);
    Task<DailyProgressEntity> MarkIncompleteAsync(string date, string taskId);
}
