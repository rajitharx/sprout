using Microsoft.EntityFrameworkCore;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.Entities;
using Sprout.DataAccess.Repositories;
using Xunit;

namespace Sprout.Api.Tests;

public class DbTaskRepositoryTests : IAsyncLifetime
{
    private readonly SproutDbContext _context;
    private readonly TaskRepository _repository;
    private readonly DbContextOptions<SproutDbContext> _options;

    public DbTaskRepositoryTests()
    {
        var databaseName = $"test_db_{Guid.NewGuid()}";
        _options = new DbContextOptionsBuilder<SproutDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        _context = new SproutDbContext(_options);
        _repository = new TaskRepository(_context);
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
    public async Task GetAllActiveAsync_ReturnsOnlyActiveTasks()
    {
        var activeTask = new HabitTaskEntity { Id = "task-1", Label = "Active", IsActive = true, SortOrder = 0 };
        var inactiveTask = new HabitTaskEntity { Id = "task-2", Label = "Inactive", IsActive = false, SortOrder = 1 };

        _context.HabitTasks.Add(activeTask);
        _context.HabitTasks.Add(inactiveTask);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllActiveAsync();

        Assert.Single(result);
        Assert.Equal("task-1", result[0].Id);
        Assert.Equal("Active", result[0].Label);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsSortedBySortOrder()
    {
        _context.HabitTasks.AddRange(
            new HabitTaskEntity { Id = "task-1", Label = "Third", SortOrder = 3, IsActive = true },
            new HabitTaskEntity { Id = "task-2", Label = "First", SortOrder = 1, IsActive = true },
            new HabitTaskEntity { Id = "task-3", Label = "Second", SortOrder = 2, IsActive = true }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllActiveAsync();

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].SortOrder);
        Assert.Equal(2, result[1].SortOrder);
        Assert.Equal(3, result[2].SortOrder);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTaskWhenExists()
    {
        var task = new HabitTaskEntity { Id = "task-1", Label = "Test Task", IsActive = true };
        _context.HabitTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync("task-1");

        Assert.NotNull(result);
        Assert.Equal("task-1", result.Id);
        Assert.Equal("Test Task", result.Label);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenNotExists()
    {
        var result = await _repository.GetByIdAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsTaskToDatabase()
    {
        var task = new HabitTaskEntity { Id = "new-task", Label = "New Task", Emoji = "🎯", SortOrder = 5, IsActive = true };

        var created = await _repository.CreateAsync(task);

        Assert.Equal("new-task", created.Id);
        Assert.Equal("New Task", created.Label);

        var dbTask = await _context.HabitTasks.FirstOrDefaultAsync(t => t.Id == "new-task");
        Assert.NotNull(dbTask);
        Assert.Equal("New Task", dbTask.Label);
    }

    [Fact]
    public async Task CreateAsync_GeneratesIdIfEmpty()
    {
        var task = new HabitTaskEntity { Label = "Task Without Id", Emoji = "🎯", IsActive = true };

        var created = await _repository.CreateAsync(task);

        Assert.False(string.IsNullOrEmpty(created.Id));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenLabelIsNull()
    {
        var task = new HabitTaskEntity { Id = "task-1", Label = "", Emoji = "🎯" };

        await Assert.ThrowsAsync<ArgumentException>(async () => await _repository.CreateAsync(task));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenEmojiIsNull()
    {
        var task = new HabitTaskEntity { Id = "task-1", Label = "Test", Emoji = "" };

        await Assert.ThrowsAsync<ArgumentException>(async () => await _repository.CreateAsync(task));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingTask()
    {
        var task = new HabitTaskEntity { Id = "task-1", Label = "Original", Emoji = "🎯", SortOrder = 1, IsActive = true };
        _context.HabitTasks.Add(task);
        await _context.SaveChangesAsync();

        var updated = new HabitTaskEntity { Id = "task-1", Label = "Updated", Emoji = "🚀", SortOrder = 2, IsActive = true };
        var result = await _repository.UpdateAsync("task-1", updated);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Label);
        Assert.Equal("🚀", result.Emoji);
        Assert.Equal(2, result.SortOrder);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullWhenTaskNotFound()
    {
        var task = new HabitTaskEntity { Label = "New Task", Emoji = "🎯" };

        var result = await _repository.UpdateAsync("nonexistent", task);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesTask()
    {
        var task = new HabitTaskEntity { Id = "task-1", Label = "To Delete", IsActive = true };
        _context.HabitTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync("task-1");

        Assert.True(result);

        var dbTask = await _context.HabitTasks.FirstOrDefaultAsync(t => t.Id == "task-1");
        Assert.NotNull(dbTask);
        Assert.False(dbTask.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalseWhenTaskNotFound()
    {
        var result = await _repository.DeleteAsync("nonexistent");

        Assert.False(result);
    }
}
