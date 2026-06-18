using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.Entities;

namespace Sprout.DataAccess.DataMigration;

public class JsonToDbMigration
{
    private readonly SproutDbContext _context;
    private readonly string _jsonDataPath;

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JsonToDbMigration(SproutDbContext context, string jsonDataPath)
    {
        _context = context;
        _jsonDataPath = jsonDataPath;
    }

    public async Task MigrateAsync()
    {
        await MigrateTasksAsync();
        await MigrateProgressAsync();
        await MigrateProfileAsync();
    }

    private async Task MigrateTasksAsync()
    {
        var tasksFile = Path.Combine(_jsonDataPath, "tasks.json");
        if (!File.Exists(tasksFile))
            return;

        var existingTasks = await _context.HabitTasks.ToListAsync();
        if (existingTasks.Any())
            return;

        var json = await File.ReadAllTextAsync(tasksFile);
        var tasks = JsonSerializer.Deserialize<List<HabitTaskEntity>>(json, _options) ?? [];

        if (tasks.Any())
        {
            _context.HabitTasks.AddRange(tasks);
            await _context.SaveChangesAsync();
        }
    }

    private async Task MigrateProgressAsync()
    {
        var progressFile = Path.Combine(_jsonDataPath, "progress.json");
        if (!File.Exists(progressFile))
            return;

        var existingProgress = await _context.DailyProgress.ToListAsync();
        if (existingProgress.Any())
            return;

        var json = await File.ReadAllTextAsync(progressFile);

        var progressRecords = JsonSerializer.Deserialize<List<JsonDailyProgress>>(json, _options) ?? [];

        foreach (var record in progressRecords)
        {
            var progress = new DailyProgressEntity
            {
                Date = record.Date,
                LastUpdated = record.LastUpdated,
                CompletedTasks = record.CompletedTaskIds
                    .Select(tid => new CompletedTaskEntity { TaskId = tid })
                    .ToList()
            };
            _context.DailyProgress.Add(progress);
        }

        if (progressRecords.Any())
            await _context.SaveChangesAsync();
    }

    private async Task MigrateProfileAsync()
    {
        var profileFile = Path.Combine(_jsonDataPath, "profile.json");
        if (!File.Exists(profileFile))
            return;

        var existingProfile = await _context.ChildProfiles.FirstOrDefaultAsync(p => p.Id == 1);
        if (existingProfile != null)
            return;

        var json = await File.ReadAllTextAsync(profileFile);
        var profile = JsonSerializer.Deserialize<JsonChildProfile>(json, _options);

        if (profile != null)
        {
            var entity = new ChildProfileEntity
            {
                Id = 1,
                Name = profile.Name,
                Avatar = profile.Avatar
            };
            _context.ChildProfiles.Add(entity);
            await _context.SaveChangesAsync();
        }
    }

    private class JsonDailyProgress
    {
        public string Date { get; set; } = "";
        public List<string> CompletedTaskIds { get; set; } = [];
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    private class JsonChildProfile
    {
        public string Name { get; set; } = "Child";
        public string Avatar { get; set; } = "👦";
    }
}
