using Microsoft.Extensions.Configuration;
using Sprout.Api.Services;
using Xunit;

namespace Sprout.Api.Tests;

public class JsonProgressServiceTests : IDisposable
{
    private readonly string _tempDir;

    public JsonProgressServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "sprout-progress-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    private JsonProgressService CreateService() =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Storage:DataPath"] = _tempDir })
            .Build());

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static string TodayString() =>
        DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");

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
    public async Task GetWeekAsync_LastDayIsToday()
    {
        var week = await CreateService().GetWeekAsync();

        Assert.Equal(TodayString(), week.Last().Date);
    }

    [Fact]
    public async Task GetWeekAsync_FirstDayIsSixDaysAgo()
    {
        var expected = DateOnly.FromDateTime(DateTime.Now).AddDays(-6).ToString("yyyy-MM-dd");
        var week = await CreateService().GetWeekAsync();

        Assert.Equal(expected, week.First().Date);
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
