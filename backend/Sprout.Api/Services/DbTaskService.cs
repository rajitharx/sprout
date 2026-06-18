using Sprout.Api.Models;
using Sprout.DataAccess.Repositories;

namespace Sprout.Api.Services;

public class DbTaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ILogger<DbTaskService> _logger;
    private readonly bool _logServiceCalls;

    public DbTaskService(ITaskRepository repository, ILogger<DbTaskService> logger, IConfiguration config)
    {
        _repository = repository;
        _logger = logger;
        _logServiceCalls = config.GetSection("Debug").GetValue<bool>("LogServiceCalls");
    }

    public async Task<List<HabitTask>> GetAllAsync()
    {
        var tasks = await _repository.GetAllActiveAsync();
        var result = tasks.Select(t => new HabitTask
        {
            Id = t.Id,
            Label = t.Label,
            Emoji = t.Emoji,
            SortOrder = t.SortOrder,
            IsActive = t.IsActive
        }).ToList();

        if (_logServiceCalls) _logger.LogDebug("📖 GetAllAsync returned {Count} tasks", result.Count);
        return result;
    }

    public async Task<HabitTask?> GetByIdAsync(string id)
    {
        var task = await _repository.GetByIdAsync(id);
        var result = task == null ? null : new HabitTask
        {
            Id = task.Id,
            Label = task.Label,
            Emoji = task.Emoji,
            SortOrder = task.SortOrder,
            IsActive = task.IsActive
        };

        if (_logServiceCalls) _logger.LogDebug("📖 GetByIdAsync({Id}) returned {Found}", id, result != null ? "found" : "not found");
        return result;
    }

    public async Task<HabitTask> CreateAsync(HabitTask task)
    {
        var entity = new Sprout.DataAccess.Entities.HabitTaskEntity
        {
            Id = task.Id,
            Label = task.Label,
            Emoji = task.Emoji,
            SortOrder = task.SortOrder,
            IsActive = task.IsActive
        };

        var created = await _repository.CreateAsync(entity);
        _logger.LogInformation("✏️ Task created: {Label} ({Emoji})", created.Label, created.Emoji);
        if (_logServiceCalls) _logger.LogDebug("  Task ID: {Id}, SortOrder: {SortOrder}", created.Id, created.SortOrder);

        return new HabitTask
        {
            Id = created.Id,
            Label = created.Label,
            Emoji = created.Emoji,
            SortOrder = created.SortOrder,
            IsActive = created.IsActive
        };
    }

    public async Task<HabitTask?> UpdateAsync(string id, HabitTask task)
    {
        var entity = new Sprout.DataAccess.Entities.HabitTaskEntity
        {
            Id = id,
            Label = task.Label,
            Emoji = task.Emoji,
            SortOrder = task.SortOrder,
            IsActive = task.IsActive
        };

        var updated = await _repository.UpdateAsync(id, entity);
        if (updated == null)
        {
            if (_logServiceCalls) _logger.LogDebug("⚠️ UpdateAsync({Id}) not found", id);
            return null;
        }

        _logger.LogInformation("✏️ Task updated: {Label} ({Emoji})", updated.Label, updated.Emoji);
        if (_logServiceCalls) _logger.LogDebug("  Task ID: {Id}, Active: {IsActive}", id, updated.IsActive);

        return new HabitTask
        {
            Id = updated.Id,
            Label = updated.Label,
            Emoji = updated.Emoji,
            SortOrder = updated.SortOrder,
            IsActive = updated.IsActive
        };
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _repository.DeleteAsync(id);
        if (result)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task != null)
                _logger.LogInformation("🗑️ Task deleted: {Label}", task.Label);
        }
        else
        {
            if (_logServiceCalls) _logger.LogDebug("⚠️ DeleteAsync({Id}) not found", id);
        }
        return result;
    }
}
