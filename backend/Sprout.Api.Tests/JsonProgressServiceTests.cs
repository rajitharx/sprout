using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sprout.Api.Services;
using Xunit;

namespace Sprout.Api.Tests;

public class JsonProgressServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly TestSystemClock _clock = new(new DateOnly(2026, 5, 30));
    private readonly ILogger<JsonProgressService> _logger;

    public JsonProgressServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "sprout-progress-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<JsonProgressService>();
    }

    private JsonProgressService CreateService() =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Storage:DataPath"] = _tempDir })
            .Build(), _clock, _logger);

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string TodayString() =>
        _clock.Today().ToString("yyyy-MM-dd");

    [Fact]
    public async Task GetTodayAsync_NewDay_ReturnsEmptyCompletedList()
    {
        var progress = await CreateService().GetTodayAsync();

        Assert.Empty(progress.CompletedTaskIds);
        Assert.Equal(TodayString(), progress.Date);
    }

    [Fact]
    public async Task MarkCompleteAsync_AddsTaskIdToToday()
    {
        var svc = CreateService();

        var progress = await svc.MarkCompleteAsync("task-1");

        Assert.Contains("task-1", progress.CompletedTaskIds);
        Assert.Equal(TodayString(), progress.Date);
    }

    [Fact]
    public async Task MarkCompleteAsync_IsIdempotent_DoesNotDuplicateEntry()
    {
        var svc = CreateService();
        await svc.MarkCompleteAsync("task-1");

        var progress = await svc.MarkCompleteAsync("task-1");

        Assert.Single(progress.CompletedTaskIds, id => id == "task-1");
    }

    [Fact]
    public async Task MarkCompleteAsync_MultipleTaskIds_AllPersisted()
    {
        var svc = CreateService();
        await svc.MarkCompleteAsync("task-a");
        var progress = await svc.MarkCompleteAsync("task-b");

        Assert.Contains("task-a", progress.CompletedTaskIds);
        Assert.Contains("task-b", progress.CompletedTaskIds);
    }

    [Fact]
    public async Task MarkCompleteAsync_UpdatesLastUpdated()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var svc = CreateService();

        var progress = await svc.MarkCompleteAsync("task-time");

        Assert.True(progress.LastUpdated >= before);
    }

    [Fact]
    public async Task MarkIncompleteAsync_RemovesTaskIdFromToday()
    {
        var svc = CreateService();
        await svc.MarkCompleteAsync("task-to-remove");

        var progress = await svc.MarkIncompleteAsync("task-to-remove");

        Assert.DoesNotContain("task-to-remove", progress.CompletedTaskIds);
    }

    [Fact]
    public async Task MarkIncompleteAsync_LeavesOtherTasksIntact()
    {
        var svc = CreateService();
        await svc.MarkCompleteAsync("task-keep");
        await svc.MarkCompleteAsync("task-remove");

        var progress = await svc.MarkIncompleteAsync("task-remove");

        Assert.Contains("task-keep", progress.CompletedTaskIds);
    }

    [Fact]
    public async Task MarkIncompleteAsync_TaskNeverCompleted_ReturnsEmptyProgress()
    {
        var progress = await CreateService().MarkIncompleteAsync("ghost-task");

        Assert.DoesNotContain("ghost-task", progress.CompletedTaskIds);
        Assert.Equal(TodayString(), progress.Date);
    }

    [Fact]
    public async Task GetTodayAsync_ReflectsCompletedTasksAfterMark()
    {
        var svc = CreateService();
        await svc.MarkCompleteAsync("task-x");

        var today = await svc.GetTodayAsync();

        Assert.Contains("task-x", today.CompletedTaskIds);
    }

    [Fact]
    public async Task GetWeekAsync_ReturnsExactlySevenDays()
    {
        var week = await CreateService().GetWeekAsync();

        Assert.Equal(7, week.Count);
    }

    [Fact]
    public async Task GetWeekAsync_DaysAreInAscendingChronologicalOrder()
    {
        var week = await CreateService().GetWeekAsync();
        var dates = week.Select(p => p.Date).ToList();

        Assert.Equal(dates.OrderBy(d => d).ToList(), dates);
    }

    [Fact]
    public async Task GetWeekAsync_LastDayIsSunday()
    {
        var week = await CreateService().GetWeekAsync();
        var today = _clock.Today();
        var daysToSunday = today.DayOfWeek == DayOfWeek.Sunday ? 0 : 7 - (int)today.DayOfWeek;
        var expectedSunday = today.AddDays(daysToSunday).ToString("yyyy-MM-dd");

        Assert.Equal(expectedSunday, week.Last().Date);
    }

    [Fact]
    public async Task GetWeekAsync_FirstDayIsMonday()
    {
        var week = await CreateService().GetWeekAsync();
        var today = _clock.Today();
        var daysToMonday = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1;
        var expectedMonday = today.AddDays(-daysToMonday).ToString("yyyy-MM-dd");

        Assert.Equal(expectedMonday, week.First().Date);
    }

    [Fact]
    public async Task GetWeekAsync_TodayEntryReflectsCompletions()
    {
        var svc = CreateService();
        await svc.MarkCompleteAsync("task-week");

        var week = await svc.GetWeekAsync();
        var todayEntry = week.First(p => p.Date == TodayString());

        Assert.Contains("task-week", todayEntry.CompletedTaskIds);
    }

    [Fact]
    public async Task GetWeekAsync_PastDaysWithNoData_ReturnEmptyCompletedLists()
    {
        var week = await CreateService().GetWeekAsync();
        var pastDays = week.Take(6);

        Assert.All(pastDays, p => Assert.Empty(p.CompletedTaskIds));
    }
}
