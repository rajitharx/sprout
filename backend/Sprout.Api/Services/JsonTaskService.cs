using System.Text.Json;
using Sprout.Api.Models;

namespace Sprout.Api.Services;

public class JsonTaskService : ITaskService
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _seeded = false;
    private readonly ILogger<JsonTaskService> _logger;
    private readonly bool _logServiceCalls;

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JsonTaskService(IConfiguration config, ILogger<JsonTaskService> logger)
    {
        _logger = logger;
        _logServiceCalls = config.GetSection("Debug").GetValue<bool>("LogServiceCalls");
        var dataPath = config["Storage:DataPath"] ?? "Storage/data";
        Directory.CreateDirectory(dataPath);
        _path = Path.Combine(dataPath, "tasks.json");
    }

    private async Task<List<HabitTask>> ReadFileAsync()
    {
        if (!File.Exists(_path)) return [];
        var json = await File.ReadAllTextAsync(_path);
        if (string.IsNullOrWhiteSpace(json)) return [];
        return JsonSerializer.Deserialize<List<HabitTask>>(json, _options) ?? [];
    }

    private async Task WriteFileAsync(List<HabitTask> data) =>
        await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(data, _options));

    private async Task EnsureSeededAsync()
    {
        if (_seeded) return;
        await _lock.WaitAsync();
        try
        {
            if (_seeded) return;
            var data = await ReadFileAsync();
            if (data.Count == 0)
            {
                data.AddRange([
                    new HabitTask { Id = "default-1", Label = "Brush Teeth",        Emoji = "🪥", SortOrder = 0, IsActive = true },
                    new HabitTask { Id = "default-2", Label = "Get Dressed",         Emoji = "👕", SortOrder = 1, IsActive = true },
                    new HabitTask { Id = "default-3", Label = "Eat Breakfast",       Emoji = "🥣", SortOrder = 2, IsActive = true },
                    new HabitTask { Id = "default-4", Label = "Eat Dinner",          Emoji = "🍽️", SortOrder = 3, IsActive = true },
                    new HabitTask { Id = "default-5", Label = "Wash Before Bed",     Emoji = "🧼", SortOrder = 4, IsActive = true },
                    new HabitTask { Id = "default-6", Label = "Brush Teeth at Night",Emoji = "🪥", SortOrder = 5, IsActive = true },
                ]);
                await WriteFileAsync(data);
            }
            _seeded = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<HabitTask>> GetAllAsync()
    {
        await EnsureSeededAsync();
        var data = await ReadFileAsync();
        var result = data.Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToList();
        if (_logServiceCalls) _logger.LogDebug("📖 GetAllAsync returned {Count} tasks", result.Count);
        return result;
    }

    public async Task<HabitTask?> GetByIdAsync(string id)
    {
        var data = await ReadFileAsync();
        var result = data.FirstOrDefault(t => t.Id == id);
        if (_logServiceCalls) _logger.LogDebug("📖 GetByIdAsync({Id}) returned {Found}", id, result != null ? "found" : "not found");
        return result;
    }

    public async Task<HabitTask> CreateAsync(HabitTask task)
    {
        if (string.IsNullOrWhiteSpace(task.Label))
            throw new ArgumentException("Task label cannot be null or empty.", nameof(task));
        if (string.IsNullOrWhiteSpace(task.Emoji))
            throw new ArgumentException("Task emoji cannot be null or empty.", nameof(task));

        await _lock.WaitAsync();
        try
        {
            var data = await ReadFileAsync();
            var taskToAdd = task with { Id = string.IsNullOrEmpty(task.Id) ? Guid.NewGuid().ToString() : task.Id };
            data.Add(taskToAdd);
            await WriteFileAsync(data);
            _logger.LogInformation("✏️ Task created: {Label} ({Emoji})", taskToAdd.Label, taskToAdd.Emoji);
            if (_logServiceCalls) _logger.LogDebug("  Task ID: {Id}, SortOrder: {SortOrder}", taskToAdd.Id, taskToAdd.SortOrder);
            return taskToAdd;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<HabitTask?> UpdateAsync(string id, HabitTask task)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Task ID cannot be null or empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(task.Label))
            throw new ArgumentException("Task label cannot be null or empty.", nameof(task));
        if (string.IsNullOrWhiteSpace(task.Emoji))
            throw new ArgumentException("Task emoji cannot be null or empty.", nameof(task));

        await _lock.WaitAsync();
        try
        {
            var data = await ReadFileAsync();
            var index = data.FindIndex(t => t.Id == id);
            if (index < 0)
            {
                if (_logServiceCalls) _logger.LogDebug("⚠️ UpdateAsync({Id}) not found", id);
                return null;
            }
            var updated = task with { Id = id };
            data[index] = updated;
            await WriteFileAsync(data);
            _logger.LogInformation("✏️ Task updated: {Label} ({Emoji})", updated.Label, updated.Emoji);
            if (_logServiceCalls) _logger.LogDebug("  Task ID: {Id}, Active: {IsActive}", id, updated.IsActive);
            return updated;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        await _lock.WaitAsync();
        try
        {
            var data = await ReadFileAsync();
            var index = data.FindIndex(t => t.Id == id);
            if (index < 0)
            {
                if (_logServiceCalls) _logger.LogDebug("⚠️ DeleteAsync({Id}) not found", id);
                return false;
            }
            var task = data[index];
            data[index] = data[index] with { IsActive = false };
            await WriteFileAsync(data);
            _logger.LogInformation("🗑️ Task deleted: {Label}", task.Label);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }
}
