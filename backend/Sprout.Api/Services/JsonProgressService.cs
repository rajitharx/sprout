using System.Text.Json;
using Sprout.Api.Models;

namespace Sprout.Api.Services;

public class JsonProgressService : IProgressService
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ISystemClock _clock;
    private readonly ILogger<JsonProgressService> _logger;
    private readonly bool _logServiceCalls;

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JsonProgressService(IConfiguration config, ISystemClock? clock = null, ILogger<JsonProgressService> logger = null!)
    {
        _logger = logger;
        _logServiceCalls = config.GetSection("Debug").GetValue<bool>("LogServiceCalls");
        var dataPath = config["Storage:DataPath"] ?? "Storage/data";
        Directory.CreateDirectory(dataPath);
        _path = Path.Combine(dataPath, "progress.json");
        if (!File.Exists(_path))
            File.WriteAllText(_path, "[]");
        _clock = clock ?? new SystemClock();
    }

    private async Task<List<DailyProgress>> ReadFileAsync()
    {
        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<List<DailyProgress>>(json, _options) ?? [];
    }

    private async Task WriteFileAsync(List<DailyProgress> data) =>
        await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(data, _options));

    private string Today() =>
        _clock.Today().ToString("yyyy-MM-dd");

    public async Task<DailyProgress> GetTodayAsync()
    {
        var data = await ReadFileAsync();
        var result = data.FirstOrDefault(p => p.Date == Today())
            ?? new DailyProgress { Date = Today() };
        if (_logServiceCalls) _logger.LogDebug("📅 GetTodayAsync returned {Count} completed tasks", result.CompletedTaskIds.Count);
        return result;
    }

    public async Task<DailyProgress> MarkCompleteAsync(string taskId)
    {
        await _lock.WaitAsync();
        try
        {
            var data = await ReadFileAsync();
            var today = Today();
            var existing = data.FirstOrDefault(p => p.Date == today);

            if (existing is null)
            {
                var created = new DailyProgress
                {
                    Date = today,
                    CompletedTaskIds = [taskId],
                    LastUpdated = DateTime.UtcNow
                };
                data.Add(created);
                await WriteFileAsync(data);
                _logger.LogInformation("✅ Task completed on {Date}", today);
                if (_logServiceCalls) _logger.LogDebug("  Task ID: {TaskId}, Total completed: 1", taskId);
                return created;
            }

            if (existing.CompletedTaskIds.Contains(taskId))
            {
                if (_logServiceCalls) _logger.LogDebug("ℹ️ MarkCompleteAsync task {TaskId} already complete", taskId);
                return existing;
            }

            var updated = existing with
            {
                CompletedTaskIds = [.. existing.CompletedTaskIds, taskId],
                LastUpdated = DateTime.UtcNow
            };
            data[data.IndexOf(existing)] = updated;
            await WriteFileAsync(data);
            _logger.LogInformation("✅ Task completed on {Date} ({Total} total)", today, updated.CompletedTaskIds.Count);
            if (_logServiceCalls) _logger.LogDebug("  Task ID: {TaskId}", taskId);
            return updated;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<DailyProgress> MarkIncompleteAsync(string taskId)
    {
        await _lock.WaitAsync();
        try
        {
            var data = await ReadFileAsync();
            var today = Today();
            var existing = data.FirstOrDefault(p => p.Date == today);

            if (existing is null)
            {
                if (_logServiceCalls) _logger.LogDebug("ℹ️ MarkIncompleteAsync no progress found for {Date}", today);
                return new DailyProgress { Date = today };
            }

            var updated = existing with
            {
                CompletedTaskIds = [.. existing.CompletedTaskIds.Where(id => id != taskId)],
                LastUpdated = DateTime.UtcNow
            };
            data[data.IndexOf(existing)] = updated;
            await WriteFileAsync(data);
            _logger.LogInformation("❌ Task marked incomplete on {Date} ({Remaining} remaining)", today, updated.CompletedTaskIds.Count);
            if (_logServiceCalls) _logger.LogDebug("  Task ID: {TaskId}", taskId);
            return updated;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<DailyProgress>> GetWeekAsync()
    {
        var data = await ReadFileAsync();
        var today = _clock.Today();
        var daysToMonday = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1;
        var monday = today.AddDays(-daysToMonday);

        var result = Enumerable.Range(0, 7)
            .Select(i => monday.AddDays(i).ToString("yyyy-MM-dd"))
            .Select(date => data.FirstOrDefault(p => p.Date == date) ?? new DailyProgress { Date = date })
            .ToList();

        if (_logServiceCalls) _logger.LogDebug("📊 GetWeekAsync returned week from {Monday} to {Sunday}", monday.ToString("yyyy-MM-dd"), monday.AddDays(6).ToString("yyyy-MM-dd"));
        return result;
    }
}
