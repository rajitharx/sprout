using System.Text.Json;
using Sprout.Api.Models;

namespace Sprout.Api.Services;

public class JsonTaskService : ITaskService
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _seeded = false;

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JsonTaskService(IConfiguration config)
    {
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
        return [.. data.Where(t => t.IsActive).OrderBy(t => t.SortOrder)];
    }

    public async Task<HabitTask?> GetByIdAsync(string id)
    {
        var data = await ReadFileAsync();
        return data.FirstOrDefault(t => t.Id == id);
    }

    public async Task<HabitTask> CreateAsync(HabitTask task)
    {
        await _lock.WaitAsync();
        try
        {
            var data = await ReadFileAsync();
            data.Add(task);
            await WriteFileAsync(data);
            return task;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<HabitTask?> UpdateAsync(string id, HabitTask task)
    {
        await _lock.WaitAsync();
        try
        {
            var data = await ReadFileAsync();
            var index = data.FindIndex(t => t.Id == id);
            if (index < 0) return null;
            var updated = task with { Id = id };
            data[index] = updated;
            await WriteFileAsync(data);
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
            if (index < 0) return false;
            data[index] = data[index] with { IsActive = false };
            await WriteFileAsync(data);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }
}
