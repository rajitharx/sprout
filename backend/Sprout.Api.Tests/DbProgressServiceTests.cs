using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Sprout.Api.Services;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.Repositories;
using Xunit;

namespace Sprout.Api.Tests;

public class DbProgressServiceTests : IAsyncLifetime
{
    private readonly SproutDbContext _context;
    private readonly TestSystemClock _clock = new(new DateOnly(2026, 5, 27));
    private readonly DbProgressService _service;

    public DbProgressServiceTests()
    {
        var options = new DbContextOptionsBuilder<SproutDbContext>()
            .UseInMemoryDatabase($"progress_svc_{Guid.NewGuid()}")
            .Options;
        _context = new SproutDbContext(options);
        _service = new DbProgressService(new ProgressRepository(_context), _clock, NullLogger<DbProgressService>.Instance);
    }

    public async Task InitializeAsync() => await _context.Database.EnsureCreatedAsync();

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Theory]
    [InlineData(2026, 5, 25)]
    [InlineData(2026, 5, 26)]
    [InlineData(2026, 5, 27)]
    [InlineData(2026, 5, 28)]
    [InlineData(2026, 5, 29)]
    [InlineData(2026, 5, 30)]
    [InlineData(2026, 5, 31)]
    public async Task GetWeekAsync_AnyDayOfWeek_ReturnsTheSameMondayToSunday(int y, int m, int d)
    {
        _clock.SetToday(new DateOnly(y, m, d));

        var week = await _service.GetWeekAsync();

        Assert.Equal(
            ["2026-05-25", "2026-05-26", "2026-05-27", "2026-05-28", "2026-05-29", "2026-05-30", "2026-05-31"],
            week.Select(p => p.Date));
    }

    [Fact]
    public async Task GetWeekAsync_Monday_StartsOnThatDay()
    {
        _clock.SetToday(new DateOnly(2026, 6, 1));

        var week = await _service.GetWeekAsync();

        Assert.Equal("2026-06-01", week.First().Date);
        Assert.Equal("2026-06-07", week.Last().Date);
    }

    [Fact]
    public async Task GetWeekAsync_Sunday_BelongsToTheWeekThatStartedSixDaysEarlier()
    {
        _clock.SetToday(new DateOnly(2026, 5, 31));

        var week = await _service.GetWeekAsync();

        Assert.Equal(DayOfWeek.Monday, DateOnly.Parse(week.First().Date).DayOfWeek);
        Assert.Equal("2026-05-31", week.Last().Date);
    }

    [Fact]
    public async Task GetWeekAsync_DaysWithoutData_AreEmptyNotMissing()
    {
        var week = await _service.GetWeekAsync();

        Assert.Equal(7, week.Count);
        Assert.All(week, p => Assert.Empty(p.CompletedTaskIds));
    }

    [Fact]
    public async Task GetWeekAsync_ShowsCompletionsOnTheDayTheyHappened()
    {
        _clock.SetToday(new DateOnly(2026, 5, 26));
        await _service.MarkCompleteAsync("tue-task");
        _clock.SetToday(new DateOnly(2026, 5, 29));

        var week = await _service.GetWeekAsync();

        Assert.Equal(["tue-task"], week.Single(p => p.Date == "2026-05-26").CompletedTaskIds);
        Assert.Empty(week.Single(p => p.Date == "2026-05-29").CompletedTaskIds);
    }

    [Fact]
    public async Task GetWeekAsync_DoesNotIncludeCompletionsFromOtherWeeks()
    {
        _clock.SetToday(new DateOnly(2026, 5, 24));
        await _service.MarkCompleteAsync("last-week");
        _clock.SetToday(new DateOnly(2026, 6, 1));
        await _service.MarkCompleteAsync("next-week");
        _clock.SetToday(new DateOnly(2026, 5, 27));

        var week = await _service.GetWeekAsync();

        Assert.All(week, p => Assert.Empty(p.CompletedTaskIds));
    }

    [Fact]
    public async Task MarkCompleteAsync_AfterMidnight_RecordsUnderTheNewDate()
    {
        _clock.SetToday(new DateOnly(2026, 5, 27));
        await _service.MarkCompleteAsync("a");
        _clock.SetToday(new DateOnly(2026, 5, 28));

        var today = await _service.GetTodayAsync();

        Assert.Equal("2026-05-28", today.Date);
        Assert.Empty(today.CompletedTaskIds);
    }

    [Fact]
    public async Task MarkCompleteAsync_AdvancesLastUpdated()
    {
        var first = await _service.MarkCompleteAsync("a");
        await Task.Delay(10);

        var second = await _service.MarkCompleteAsync("b");

        Assert.True(second.LastUpdated > first.LastUpdated);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task MarkCompleteAsync_BlankId_Throws(string id)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.MarkCompleteAsync(id));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task MarkIncompleteAsync_BlankId_Throws(string id)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.MarkIncompleteAsync(id));
    }
}
