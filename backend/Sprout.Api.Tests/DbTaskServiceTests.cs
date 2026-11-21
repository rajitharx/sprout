using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Sprout.Api.Models;
using Sprout.Api.Services;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.Repositories;
using Xunit;

namespace Sprout.Api.Tests;

public class DbTaskServiceTests : IAsyncLifetime
{
    private readonly SproutDbContext _context;
    private readonly DbTaskService _service;

    public DbTaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<SproutDbContext>()
            .UseInMemoryDatabase($"task_svc_{Guid.NewGuid()}")
            .Options;
        _context = new SproutDbContext(options);
        _service = new DbTaskService(
            new TaskRepository(_context),
            NullLogger<DbTaskService>.Instance,
            new ConfigurationBuilder().Build());
    }

    public async Task InitializeAsync() => await _context.Database.EnsureCreatedAsync();

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task UpdateAsync_KeepsRouteIdEvenWhenPayloadCarriesAnotherId()
    {
        var created = await _service.CreateAsync(new HabitTask { Label = "Old", Emoji = "🪥" });

        var updated = await _service.UpdateAsync(created.Id, new HabitTask { Id = "other", Label = "New", Emoji = "🛁" });

        Assert.Equal(created.Id, updated!.Id);
        Assert.Equal("New", (await _service.GetByIdAsync(created.Id))!.Label);
        Assert.Null(await _service.GetByIdAsync("other"));
    }

    [Fact]
    public async Task GetAllAsync_OmitsSoftDeletedTasksAndOrdersBySortOrder()
    {
        var b = await _service.CreateAsync(new HabitTask { Label = "B", Emoji = "🅱️", SortOrder = 2 });
        await _service.CreateAsync(new HabitTask { Label = "A", Emoji = "🅰️", SortOrder = 1 });
        var gone = await _service.CreateAsync(new HabitTask { Label = "Gone", Emoji = "❌", SortOrder = 0 });
        await _service.DeleteAsync(gone.Id);

        var all = await _service.GetAllAsync();

        Assert.Equal(["A", "B"], all.Select(t => t.Label));
        Assert.DoesNotContain(all, t => t.Id == gone.Id);
        Assert.Contains(all, t => t.Id == b.Id);
    }
}
