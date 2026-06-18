using Microsoft.EntityFrameworkCore;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.Entities;
using Sprout.DataAccess.Repositories;
using Xunit;

namespace Sprout.Api.Tests;

public class DbProgressRepositoryTests : IAsyncLifetime
{
    private readonly SproutDbContext _context;
    private readonly ProgressRepository _repository;
    private readonly DbContextOptions<SproutDbContext> _options;

    public DbProgressRepositoryTests()
    {
        var databaseName = $"test_db_{Guid.NewGuid()}";
        _options = new DbContextOptionsBuilder<SproutDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        _context = new SproutDbContext(_options);
        _repository = new ProgressRepository(_context);
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetByDateAsync_ReturnsProgressWhenExists()
    {
        var progress = new DailyProgressEntity
        {
            Date = "2026-06-18",
            CompletedTasks = [new CompletedTaskEntity { TaskId = "task-1" }],
            LastUpdated = DateTime.UtcNow
        };
        _context.DailyProgress.Add(progress);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByDateAsync("2026-06-18");

        Assert.NotNull(result);
        Assert.Equal("2026-06-18", result.Date);
        Assert.Single(result.CompletedTasks);
        Assert.Equal("task-1", result.CompletedTasks[0].TaskId);
    }

    [Fact]
    public async Task GetByDateAsync_ReturnsNullWhenNotExists()
    {
        var result = await _repository.GetByDateAsync("2026-06-18");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_CreatesNewProgress()
    {
        var result = await _repository.CreateOrUpdateAsync("2026-06-18", ["task-1", "task-2"]);

        Assert.NotNull(result);
        Assert.Equal("2026-06-18", result.Date);
        Assert.Equal(2, result.CompletedTasks.Count);
        Assert.Contains(result.CompletedTasks, c => c.TaskId == "task-1");
        Assert.Contains(result.CompletedTasks, c => c.TaskId == "task-2");
    }

    [Fact]
    public async Task CreateOrUpdateAsync_UpdatesExistingProgress()
    {
        var progress = new DailyProgressEntity
        {
            Date = "2026-06-18",
            CompletedTasks = [new CompletedTaskEntity { TaskId = "task-1" }],
            LastUpdated = DateTime.UtcNow
        };
        _context.DailyProgress.Add(progress);
        await _context.SaveChangesAsync();

        var result = await _repository.CreateOrUpdateAsync("2026-06-18", ["task-2", "task-3"]);

        Assert.Equal("2026-06-18", result.Date);
        Assert.Equal(2, result.CompletedTasks.Count);
        Assert.Contains(result.CompletedTasks, c => c.TaskId == "task-2");
        Assert.Contains(result.CompletedTasks, c => c.TaskId == "task-3");
    }

    [Fact]
    public async Task MarkCompleteAsync_CreatesProgressIfNotExists()
    {
        var result = await _repository.MarkCompleteAsync("2026-06-18", "task-1");

        Assert.NotNull(result);
        Assert.Equal("2026-06-18", result.Date);
        Assert.Single(result.CompletedTasks);
        Assert.Equal("task-1", result.CompletedTasks[0].TaskId);
    }

    [Fact]
    public async Task MarkCompleteAsync_AddsTaskIfNotAlreadyComplete()
    {
        var progress = new DailyProgressEntity
        {
            Date = "2026-06-18",
            CompletedTasks = [new CompletedTaskEntity { TaskId = "task-1" }],
            LastUpdated = DateTime.UtcNow
        };
        _context.DailyProgress.Add(progress);
        await _context.SaveChangesAsync();

        var result = await _repository.MarkCompleteAsync("2026-06-18", "task-2");

        Assert.Equal(2, result.CompletedTasks.Count);
        Assert.Contains(result.CompletedTasks, c => c.TaskId == "task-1");
        Assert.Contains(result.CompletedTasks, c => c.TaskId == "task-2");
    }

    [Fact]
    public async Task MarkCompleteAsync_DoesNotDuplicateIfAlreadyComplete()
    {
        var progress = new DailyProgressEntity
        {
            Date = "2026-06-18",
            CompletedTasks = [new CompletedTaskEntity { TaskId = "task-1" }],
            LastUpdated = DateTime.UtcNow
        };
        _context.DailyProgress.Add(progress);
        await _context.SaveChangesAsync();

        var result = await _repository.MarkCompleteAsync("2026-06-18", "task-1");

        Assert.Single(result.CompletedTasks);
    }

    [Fact]
    public async Task MarkIncompleteAsync_RemovesTaskIfExists()
    {
        var progress = new DailyProgressEntity
        {
            Date = "2026-06-18",
            CompletedTasks =
            [
                new CompletedTaskEntity { TaskId = "task-1" },
                new CompletedTaskEntity { TaskId = "task-2" }
            ],
            LastUpdated = DateTime.UtcNow
        };
        _context.DailyProgress.Add(progress);
        await _context.SaveChangesAsync();

        var result = await _repository.MarkIncompleteAsync("2026-06-18", "task-1");

        Assert.Single(result.CompletedTasks);
        Assert.Equal("task-2", result.CompletedTasks[0].TaskId);
    }

    [Fact]
    public async Task MarkIncompleteAsync_ReturnsEmptyProgressIfDateNotFound()
    {
        var result = await _repository.MarkIncompleteAsync("2026-06-18", "task-1");

        Assert.NotNull(result);
        Assert.Equal("2026-06-18", result.Date);
        Assert.Empty(result.CompletedTasks);
    }

    [Fact]
    public async Task GetWeekAsync_ReturnsProgressForDateRange()
    {
        var progress1 = new DailyProgressEntity { Date = "2026-06-16", CompletedTasks = [], LastUpdated = DateTime.UtcNow };
        var progress2 = new DailyProgressEntity { Date = "2026-06-18", CompletedTasks = [], LastUpdated = DateTime.UtcNow };

        _context.DailyProgress.Add(progress1);
        _context.DailyProgress.Add(progress2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetWeekAsync("2026-06-16", "2026-06-22");

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Date == "2026-06-16");
        Assert.Contains(result, p => p.Date == "2026-06-18");
    }

    [Fact]
    public async Task GetWeekAsync_ReturnsInOrderByDate()
    {
        _context.DailyProgress.Add(new DailyProgressEntity { Date = "2026-06-18", CompletedTasks = [], LastUpdated = DateTime.UtcNow });
        _context.DailyProgress.Add(new DailyProgressEntity { Date = "2026-06-16", CompletedTasks = [], LastUpdated = DateTime.UtcNow });
        _context.DailyProgress.Add(new DailyProgressEntity { Date = "2026-06-17", CompletedTasks = [], LastUpdated = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _repository.GetWeekAsync("2026-06-16", "2026-06-18");

        Assert.Equal("2026-06-16", result[0].Date);
        Assert.Equal("2026-06-17", result[1].Date);
        Assert.Equal("2026-06-18", result[2].Date);
    }
}
