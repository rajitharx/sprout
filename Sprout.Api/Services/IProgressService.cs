using Sprout.Api.Models;

namespace Sprout.Api.Services;

public interface IProgressService
{
    Task<DailyProgress> GetTodayAsync();
    Task<DailyProgress> MarkCompleteAsync(string taskId);
    Task<DailyProgress> MarkIncompleteAsync(string taskId);
    Task<List<DailyProgress>> GetWeekAsync();
}
