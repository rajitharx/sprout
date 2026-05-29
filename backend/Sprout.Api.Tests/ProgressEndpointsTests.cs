using System.Net;
using System.Net.Http.Json;
using Sprout.Api.Models;
using Xunit;

namespace Sprout.Api.Tests;

public class ProgressEndpointsTests : IClassFixture<SproutWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly DateOnly TestToday = new(2026, 5, 30);

    public ProgressEndpointsTests(SproutWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string TodayString() =>
        TestToday.ToString("yyyy-MM-dd");

    [Fact]
    public async Task GetToday_Returns200WithTodaysDate()
    {
        var response = await _client.GetAsync("/api/progress/today");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var progress = await response.Content.ReadFromJsonAsync<DailyProgress>();
        Assert.NotNull(progress);
        Assert.Equal(TodayString(), progress.Date);
    }

    [Fact]
    public async Task MarkComplete_Returns200WithTaskInCompletedList()
    {
        var taskId = "complete-test-" + Guid.NewGuid().ToString("N");

        var response = await _client.PostAsync($"/api/progress/complete/{taskId}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var progress = await response.Content.ReadFromJsonAsync<DailyProgress>();
        Assert.Contains(taskId, progress!.CompletedTaskIds);
    }

    [Fact]
    public async Task MarkComplete_CalledTwice_IsIdempotent()
    {
        var taskId = "idempotent-" + Guid.NewGuid().ToString("N");
        await _client.PostAsync($"/api/progress/complete/{taskId}", null);

        var response = await _client.PostAsync($"/api/progress/complete/{taskId}", null);
        var progress = await response.Content.ReadFromJsonAsync<DailyProgress>();

        Assert.Single(progress!.CompletedTaskIds, id => id == taskId);
    }

    [Fact]
    public async Task MarkComplete_ReflectedInGetToday()
    {
        var taskId = "reflect-today-" + Guid.NewGuid().ToString("N");
        await _client.PostAsync($"/api/progress/complete/{taskId}", null);

        var today = await _client.GetFromJsonAsync<DailyProgress>("/api/progress/today");

        Assert.Contains(taskId, today!.CompletedTaskIds);
    }

    [Fact]
    public async Task MarkIncomplete_Returns200WithTaskRemovedFromList()
    {
        var taskId = "incomplete-test-" + Guid.NewGuid().ToString("N");
        await _client.PostAsync($"/api/progress/complete/{taskId}", null);

        var response = await _client.PostAsync($"/api/progress/incomplete/{taskId}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var progress = await response.Content.ReadFromJsonAsync<DailyProgress>();
        Assert.DoesNotContain(taskId, progress!.CompletedTaskIds);
    }

    [Fact]
    public async Task MarkIncomplete_TaskNeverCompleted_Returns200WithEmptyList()
    {
        var taskId = "never-completed-" + Guid.NewGuid().ToString("N");

        var response = await _client.PostAsync($"/api/progress/incomplete/{taskId}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var progress = await response.Content.ReadFromJsonAsync<DailyProgress>();
        Assert.DoesNotContain(taskId, progress!.CompletedTaskIds);
    }

    [Fact]
    public async Task MarkIncomplete_DoesNotAffectOtherCompletedTasks()
    {
        var keepId = "keep-" + Guid.NewGuid().ToString("N");
        var removeId = "remove-" + Guid.NewGuid().ToString("N");
        await _client.PostAsync($"/api/progress/complete/{keepId}", null);
        await _client.PostAsync($"/api/progress/complete/{removeId}", null);

        await _client.PostAsync($"/api/progress/incomplete/{removeId}", null);

        var today = await _client.GetFromJsonAsync<DailyProgress>("/api/progress/today");
        Assert.Contains(keepId, today!.CompletedTaskIds);
        Assert.DoesNotContain(removeId, today.CompletedTaskIds);
    }

    [Fact]
    public async Task GetWeek_Returns200WithSevenDays()
    {
        var response = await _client.GetAsync("/api/progress/week");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var week = await response.Content.ReadFromJsonAsync<List<DailyProgress>>();
        Assert.NotNull(week);
        Assert.Equal(7, week.Count);
    }

    [Fact]
    public async Task GetWeek_DaysAreInAscendingChronologicalOrder()
    {
        var week = await _client.GetFromJsonAsync<List<DailyProgress>>("/api/progress/week");
        var dates = week!.Select(p => p.Date).ToList();

        Assert.Equal(dates.OrderBy(d => d).ToList(), dates);
    }

    [Fact]
    public async Task GetWeek_LastDayIsSunday()
    {
        var week = await _client.GetFromJsonAsync<List<DailyProgress>>("/api/progress/week");
        var daysToSunday = TestToday.DayOfWeek == DayOfWeek.Sunday ? 0 : 7 - (int)TestToday.DayOfWeek;
        var expectedSunday = TestToday.AddDays(daysToSunday).ToString("yyyy-MM-dd");

        Assert.Equal(expectedSunday, week!.Last().Date);
    }

    [Fact]
    public async Task GetWeek_TodayEntryReflectsCompletions()
    {
        var taskId = "week-reflect-" + Guid.NewGuid().ToString("N");
        await _client.PostAsync($"/api/progress/complete/{taskId}", null);

        var week = await _client.GetFromJsonAsync<List<DailyProgress>>("/api/progress/week");
        var todayEntry = week!.First(p => p.Date == TodayString());

        Assert.Contains(taskId, todayEntry.CompletedTaskIds);
    }
}
