using Sprout.Api.Models;
using Sprout.DataAccess.Repositories;

namespace Sprout.Api.Services;

public class DbProgressService : IProgressService
{
    private readonly IProgressRepository _repository;
    private readonly ISystemClock _clock;
    private readonly ILogger<DbProgressService> _logger;
    private readonly bool _logServiceCalls;

    public DbProgressService(IProgressRepository repository, ISystemClock? clock = null, ILogger<DbProgressService> logger = null!, IConfiguration config = null!)
    {
        _repository = repository;
        _clock = clock ?? new SystemClock();
        _logger = logger;
        _logServiceCalls = config?.GetSection("Debug").GetValue<bool>("LogServiceCalls") ?? false;
    }

    private string Today() =>
        _clock.Today().ToString("yyyy-MM-dd");

    private DailyProgress MapToDomain(Sprout.DataAccess.Entities.DailyProgressEntity entity)
    {
        return new DailyProgress
        {
            Date = entity.Date,
            CompletedTaskIds = entity.CompletedTasks.Select(c => c.TaskId).ToList(),
            LastUpdated = entity.LastUpdated
        };
    }

    public async Task<DailyProgress> GetTodayAsync()
    {
        var today = Today();
        var entity = await _repository.GetByDateAsync(today);
        var result = entity != null ? MapToDomain(entity) : new DailyProgress { Date = today };

        if (_logServiceCalls) _logger.LogDebug("📅 GetTodayAsync returned {Count} completed tasks", result.CompletedTaskIds.Count);
        return result;
    }

    public async Task<DailyProgress> MarkCompleteAsync(string taskId)
    {
        if (string.IsNullOrWhiteSpace(taskId))
            throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));

        var today = Today();
        var entity = await _repository.MarkCompleteAsync(today, taskId);
        var result = MapToDomain(entity);

        _logger.LogInformation("✅ Task completed on {Date}", today);
        if (_logServiceCalls) _logger.LogDebug("  Task ID: {TaskId}, Total completed: {Total}", taskId, result.CompletedTaskIds.Count);

        return result;
    }

    public async Task<DailyProgress> MarkIncompleteAsync(string taskId)
    {
        if (string.IsNullOrWhiteSpace(taskId))
            throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));

        var today = Today();
        var entity = await _repository.MarkIncompleteAsync(today, taskId);
        var result = MapToDomain(entity);

        _logger.LogInformation("❌ Task marked incomplete on {Date} ({Remaining} remaining)", today, result.CompletedTaskIds.Count);
        if (_logServiceCalls) _logger.LogDebug("  Task ID: {TaskId}", taskId);

        return result;
    }

    public async Task<List<DailyProgress>> GetWeekAsync()
    {
        var today = _clock.Today();
        var daysToMonday = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1;
        var monday = today.AddDays(-daysToMonday);
        var sunday = monday.AddDays(6);

        var startDate = monday.ToString("yyyy-MM-dd");
        var endDate = sunday.ToString("yyyy-MM-dd");

        var entities = await _repository.GetWeekAsync(startDate, endDate);

        var result = Enumerable.Range(0, 7)
            .Select(i => monday.AddDays(i).ToString("yyyy-MM-dd"))
            .Select(date => entities.FirstOrDefault(e => e.Date == date) != null
                ? MapToDomain(entities.First(e => e.Date == date))
                : new DailyProgress { Date = date })
            .ToList();

        if (_logServiceCalls) _logger.LogDebug("📊 GetWeekAsync returned week from {Monday} to {Sunday}", startDate, endDate);
        return result;
    }
}
