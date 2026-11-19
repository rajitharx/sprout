using Microsoft.EntityFrameworkCore;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.DataMigration;
using Sprout.DataAccess.Entities;
using Xunit;

namespace Sprout.Api.Tests;

public class JsonToDbMigrationTests : IAsyncLifetime
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "sprout-migration-" + Guid.NewGuid().ToString("N"));
    private readonly SproutDbContext _context;

    public JsonToDbMigrationTests()
    {
        Directory.CreateDirectory(_dir);
        var options = new DbContextOptionsBuilder<SproutDbContext>()
            .UseInMemoryDatabase($"migration_{Guid.NewGuid()}")
            .Options;
        _context = new SproutDbContext(options);
    }

    public async Task InitializeAsync() => await _context.Database.EnsureCreatedAsync();

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, recursive: true);
    }

    private void Write(string file, string json) => File.WriteAllText(Path.Combine(_dir, file), json);

    private Task Migrate() => new JsonToDbMigration(_context, _dir).MigrateAsync();

    [Fact]
    public async Task Migrate_NoJsonFiles_LeavesDatabaseUntouched()
    {
        await Migrate();

        Assert.Empty(await _context.HabitTasks.ToListAsync());
        Assert.Empty(await _context.DailyProgress.ToListAsync());
        Assert.Empty(await _context.ChildProfiles.ToListAsync());
    }

    [Fact]
    public async Task Migrate_TasksFile_ImportsTasks()
    {
        Write("tasks.json", """[{"id":"t1","label":"Brush","emoji":"🪥","sortOrder":1,"isActive":true}]""");

        await Migrate();

        var task = Assert.Single(await _context.HabitTasks.ToListAsync());
        Assert.Equal("t1", task.Id);
        Assert.Equal("Brush", task.Label);
    }

    [Fact]
    public async Task Migrate_TasksAlreadyInDatabase_DoesNotImportAgain()
    {
        _context.HabitTasks.Add(new HabitTaskEntity { Id = "existing", Label = "Keep", Emoji = "✅" });
        await _context.SaveChangesAsync();
        Write("tasks.json", """[{"id":"t1","label":"Brush","emoji":"🪥"}]""");

        await Migrate();

        var task = Assert.Single(await _context.HabitTasks.ToListAsync());
        Assert.Equal("existing", task.Id);
    }

    [Fact]
    public async Task Migrate_EmptyTasksArray_ImportsNothing()
    {
        Write("tasks.json", "[]");

        await Migrate();

        Assert.Empty(await _context.HabitTasks.ToListAsync());
    }

    [Fact]
    public async Task Migrate_ProgressFile_ImportsDaysWithCompletedTasks()
    {
        Write("progress.json", """[{"date":"2026-05-30","completedTaskIds":["a","b"],"lastUpdated":"2026-05-30T10:00:00Z"}]""");

        await Migrate();

        var day = await _context.DailyProgress.Include(p => p.CompletedTasks).SingleAsync();
        Assert.Equal("2026-05-30", day.Date);
        Assert.Equal(["a", "b"], day.CompletedTasks.Select(c => c.TaskId).OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task Migrate_ProgressAlreadyInDatabase_DoesNotImportAgain()
    {
        _context.DailyProgress.Add(new DailyProgressEntity { Date = "2026-01-01" });
        await _context.SaveChangesAsync();
        Write("progress.json", """[{"date":"2026-05-30","completedTaskIds":[]}]""");

        await Migrate();

        var day = Assert.Single(await _context.DailyProgress.ToListAsync());
        Assert.Equal("2026-01-01", day.Date);
    }

    [Fact]
    public async Task Migrate_ProfileFile_ImportsProfileWithIdOne()
    {
        Write("profile.json", """{"name":"Mila","avatar":"🐻"}""");

        await Migrate();

        var profile = await _context.ChildProfiles.SingleAsync();
        Assert.Equal(1, profile.Id);
        Assert.Equal("Mila", profile.Name);
        Assert.Equal("🐻", profile.Avatar);
    }

    [Fact]
    public async Task Migrate_ProfileFileMissingFields_UsesDefaults()
    {
        Write("profile.json", "{}");

        await Migrate();

        var profile = await _context.ChildProfiles.SingleAsync();
        Assert.Equal("Child", profile.Name);
        Assert.Equal("👦", profile.Avatar);
    }

    [Fact]
    public async Task Migrate_ProfileAlreadyInDatabase_DoesNotOverwrite()
    {
        _context.ChildProfiles.Add(new ChildProfileEntity { Id = 1, Name = "Original", Avatar = "🐼" });
        await _context.SaveChangesAsync();
        Write("profile.json", """{"name":"Mila","avatar":"🐻"}""");

        await Migrate();

        Assert.Equal("Original", (await _context.ChildProfiles.SingleAsync()).Name);
    }

    [Fact]
    public async Task Migrate_RunTwice_IsIdempotent()
    {
        Write("tasks.json", """[{"id":"t1","label":"Brush","emoji":"🪥"}]""");
        Write("progress.json", """[{"date":"2026-05-30","completedTaskIds":["t1"]}]""");
        Write("profile.json", """{"name":"Mila","avatar":"🐻"}""");

        await Migrate();
        await Migrate();

        Assert.Single(await _context.HabitTasks.ToListAsync());
        Assert.Single(await _context.DailyProgress.ToListAsync());
        Assert.Single(await _context.ChildProfiles.ToListAsync());
    }
}
