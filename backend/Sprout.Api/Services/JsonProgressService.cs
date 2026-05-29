using System.Text.Json;
using Sprout.Api.Models;

namespace Sprout.Api.Services;

public class JsonProgressService : IProgressService
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JsonProgressService(IConfiguration config)
    {
        var dataPath = config["Storage:DataPath"] ?? "Storage/data";
        Directory.CreateDirectory(dataPath);
        _path = Path.Combine(dataPath, "progress.json");
        if (!File.Exists(_path))
            File.WriteAllText(_path, "[]");
    }

    private async Task<List<DailyProgress>> ReadFileAsync()
    {
        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<List<DailyProgress>>(json, _options) ?? [];
    }

    private async Task WriteFileAsync(List<DailyProgress> data) =>
        await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(data, _options));

    private static string Today() =>
        DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");

    public async Task<DailyProgress> GetTodayAsync()
    {
        var data = await ReadFileAsync();
        return data.FirstOrDefault(p => p.Date == Today())
            ?? new DailyProgress { Date = Today() };
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
                return created;
            }

            if (existing.CompletedTaskIds.Contains(taskId))
                return existing;

            var updated = existing with
            {
                CompletedTaskIds = [.. existing.CompletedTaskIds, taskId],
                LastUpdated = DateTime.UtcNow
            };
            data[data.IndexOf(existing)] = updated;
            await WriteFileAsync(data);
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
                return new DailyProgress { Date = today };

            var updated = existing with
            {
                CompletedTaskIds = [.. existing.CompletedTaskIds.Where(id => id != taskId)],
                LastUpdated = DateTime.UtcNow
            };
            data[data.IndexOf(existing)] = updated;
            await WriteFileAsync(data);
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
        var today = DateOnly.FromDateTime(DateTime.Now);
        var daysToMonday = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1;
        var monday = today.AddDays(-daysToMonday);

        return Enumerable.Range(0, 7)
            .Select(i => monday.AddDays(i).ToString("yyyy-MM-dd"))
            .Select(date => data.FirstOrDefault(p => p.Date == date) ?? new DailyProgress { Date = date })
            .ToList();
    }
}
