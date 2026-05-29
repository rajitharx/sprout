using System.Net;
using System.Net.Http.Json;
using Sprout.Api.Models;
using Xunit;

namespace Sprout.Api.Tests;

public class TaskEndpointsTests : IClassFixture<SproutWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TaskEndpointsTests(SproutWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTasks_Returns200WithTaskList()
    {
        var response = await _client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<List<HabitTask>>();
        Assert.NotNull(tasks);
    }

    [Fact]
    public async Task PostTask_Returns201WithCreatedTask()
    {
        var newTask = new HabitTask { Label = "Bath Time", Emoji = "🛁", SortOrder = 5 };

        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<HabitTask>();
        Assert.NotNull(created);
        Assert.Equal("Bath Time", created.Label);
        Assert.Equal("🛁", created.Emoji);
    }

    [Fact]
    public async Task PostTask_LocationHeaderPointsToNewResource()
    {
        var newTask = new HabitTask { Label = "Shoes On", Emoji = "👟" };

        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var created = await response.Content.ReadFromJsonAsync<HabitTask>();

        Assert.Contains($"/api/tasks/{created!.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task PostTask_NewTaskAppearsInGetTasks()
    {
        var newTask = new HabitTask { Label = "Unique Task " + Guid.NewGuid(), Emoji = "🧸" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var created = await createResponse.Content.ReadFromJsonAsync<HabitTask>();

        var tasks = await _client.GetFromJsonAsync<List<HabitTask>>("/api/tasks");

        Assert.Contains(tasks!, t => t.Id == created!.Id);
    }

    [Fact]
    public async Task PutTask_ExistingId_Returns200WithUpdatedData()
    {
        var task = new HabitTask { Label = "Before Update", Emoji = "📚" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", task);
        var created = await createResponse.Content.ReadFromJsonAsync<HabitTask>();

        var response = await _client.PutAsJsonAsync(
            $"/api/tasks/{created!.Id}",
            created with { Label = "After Update", Emoji = "🎨" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<HabitTask>();
        Assert.Equal("After Update", updated!.Label);
        Assert.Equal("🎨", updated.Emoji);
        Assert.Equal(created.Id, updated.Id);
    }

    [Fact]
    public async Task PutTask_NonexistentId_Returns404()
    {
        var response = await _client.PutAsJsonAsync(
            "/api/tasks/nonexistent-id",
            new HabitTask { Label = "Ghost" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_ExistingId_Returns204()
    {
        var task = new HabitTask { Label = "Will Be Deleted " + Guid.NewGuid() };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", task);
        var created = await createResponse.Content.ReadFromJsonAsync<HabitTask>();

        var response = await _client.DeleteAsync($"/api/tasks/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_NonexistentId_Returns404()
    {
        var response = await _client.DeleteAsync("/api/tasks/nonexistent-id");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_SoftDeletes_TaskAbsentFromGetTasks()
    {
        var task = new HabitTask { Label = "Soft Delete Test " + Guid.NewGuid() };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", task);
        var created = await createResponse.Content.ReadFromJsonAsync<HabitTask>();

        await _client.DeleteAsync($"/api/tasks/{created!.Id}");

        var tasks = await _client.GetFromJsonAsync<List<HabitTask>>("/api/tasks");
        Assert.DoesNotContain(tasks!, t => t.Id == created.Id);
    }
}
