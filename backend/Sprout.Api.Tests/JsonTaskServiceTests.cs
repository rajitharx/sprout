using Microsoft.Extensions.Configuration;
using Sprout.Api.Models;
using Sprout.Api.Services;
using Xunit;

namespace Sprout.Api.Tests;

public class JsonTaskServiceTests : IDisposable
{
    private readonly string _tempDir;

    public JsonTaskServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "sprout-task-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    private JsonTaskService CreateService() =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Storage:DataPath"] = _tempDir })
            .Build());

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task GetAllAsync_OnFirstRun_SeedsDefaultTasks()
    {
        var svc = CreateService();

        var tasks = await svc.GetAllAsync();

        Assert.Equal(6, tasks.Count);
        Assert.Equal("default-1", tasks[0].Id);
        Assert.Equal("Brush Teeth", tasks[0].Label);
        Assert.Equal("🪥", tasks[0].Emoji);
        Assert.Equal("default-6", tasks[5].Id);
        Assert.Equal("Brush Teeth at Night", tasks[5].Label);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveTasks()
    {
        var svc = CreateService();
        await svc.CreateAsync(new HabitTask { Label = "Active Task", IsActive = true });
        await svc.CreateAsync(new HabitTask { Label = "Inactive Task", IsActive = false });

        var tasks = await svc.GetAllAsync();

        Assert.DoesNotContain(tasks, t => t.Label == "Inactive Task");
        Assert.Contains(tasks, t => t.Label == "Active Task");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTasksOrderedBySortOrder()
    {
        var svc = CreateService();
        await svc.CreateAsync(new HabitTask { Label = "Last", SortOrder = 10 });
        await svc.CreateAsync(new HabitTask { Label = "First", SortOrder = 1 });
        await svc.CreateAsync(new HabitTask { Label = "Middle", SortOrder = 5 });

        var tasks = await svc.GetAllAsync();
        var orders = tasks.Select(t => t.SortOrder).ToList();

        Assert.Equal(orders.OrderBy(o => o).ToList(), orders);
    }

    [Fact]
    public async Task CreateAsync_AddsTaskAndReturnsIt()
    {
        var svc = CreateService();
        var task = new HabitTask { Label = "Bath Time", Emoji = "🛁", SortOrder = 2 };

        var created = await svc.CreateAsync(task);

        Assert.Equal(task.Id, created.Id);
        Assert.Equal("Bath Time", created.Label);
        Assert.Equal("🛁", created.Emoji);
    }

    [Fact]
    public async Task CreateAsync_PersistedTask_AppearsInGetAll()
    {
        var svc = CreateService();
        var task = new HabitTask { Label = "Story Time", Emoji = "📚" };
        await svc.CreateAsync(task);

        var all = await svc.GetAllAsync();

        Assert.Contains(all, t => t.Id == task.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMatchingTask()
    {
        var svc = CreateService();
        var task = new HabitTask { Label = "Put Shoes On", Emoji = "👟" };
        await svc.CreateAsync(task);

        var found = await svc.GetByIdAsync(task.Id);

        Assert.NotNull(found);
        Assert.Equal(task.Id, found.Id);
        Assert.Equal("Put Shoes On", found.Label);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await CreateService().GetByIdAsync("does-not-exist");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ChangesLabelAndEmoji()
    {
        var svc = CreateService();
        var task = new HabitTask { Label = "Original" };
        await svc.CreateAsync(task);

        var updated = await svc.UpdateAsync(task.Id, task with { Label = "Updated", Emoji = "🎨" });

        Assert.NotNull(updated);
        Assert.Equal(task.Id, updated.Id);
        Assert.Equal("Updated", updated.Label);
        Assert.Equal("🎨", updated.Emoji);
    }

    [Fact]
    public async Task UpdateAsync_PresevesIdRegardlessOfPayload()
    {
        var svc = CreateService();
        var task = new HabitTask { Label = "Original" };
        await svc.CreateAsync(task);

        var updated = await svc.UpdateAsync(task.Id, new HabitTask { Id = "different-id", Label = "New" });

        Assert.Equal(task.Id, updated!.Id);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsNull()
    {
        var result = await CreateService().UpdateAsync("ghost", new HabitTask { Label = "Ghost" });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesTask_SetIsActiveFalse()
    {
        var svc = CreateService();
        var task = new HabitTask { Label = "To Delete" };
        await svc.CreateAsync(task);

        var result = await svc.DeleteAsync(task.Id);

        Assert.True(result);
        var stored = await svc.GetByIdAsync(task.Id);
        Assert.NotNull(stored);
        Assert.False(stored.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletedTask_NotReturnedByGetAll()
    {
        var svc = CreateService();
        var task = new HabitTask { Label = "Hidden After Delete" };
        await svc.CreateAsync(task);

        await svc.DeleteAsync(task.Id);
        var all = await svc.GetAllAsync();

        Assert.DoesNotContain(all, t => t.Id == task.Id);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        var result = await CreateService().DeleteAsync("nonexistent");

        Assert.False(result);
    }
}
