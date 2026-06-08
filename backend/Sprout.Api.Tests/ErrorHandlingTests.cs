using System.Net;
using System.Net.Http.Json;
using Sprout.Api.Models;
using Xunit;

namespace Sprout.Api.Tests;

public class ErrorHandlingTests : IClassFixture<SproutWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ErrorHandlingTests(SproutWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostTask_EmptyLabel_ReturnsBadRequest()
    {
        var task = new HabitTask { Label = "", Emoji = "🪥" };
        var response = await _client.PostAsJsonAsync("/api/tasks", task);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error?.Error);
        Assert.NotNull(error!.Error.ValidationErrors);
        Assert.Contains("label", error.Error.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task PostTask_NullLabel_ReturnsBadRequest()
    {
        var json = """{"emoji":"🪥","sortOrder":0,"isActive":true}""";
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/tasks", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTask_EmptyEmoji_ReturnsBadRequest()
    {
        var task = new HabitTask { Label = "Brush Teeth", Emoji = "" };
        var response = await _client.PostAsJsonAsync("/api/tasks", task);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error?.Error);
        Assert.NotNull(error!.Error.ValidationErrors);
        Assert.Contains("emoji", error.Error.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task PostTask_LabelTooLong_ReturnsBadRequest()
    {
        var longLabel = new string('a', 101);
        var task = new HabitTask { Label = longLabel, Emoji = "🪥" };
        var response = await _client.PostAsJsonAsync("/api/tasks", task);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTask_NegativeSortOrder_ReturnsBadRequest()
    {
        var task = new HabitTask { Label = "Test", Emoji = "🪥", SortOrder = -1 };
        var response = await _client.PostAsJsonAsync("/api/tasks", task);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTask_EmptyLabel_ReturnsBadRequest()
    {
        var newTask = new HabitTask { Label = "Original", Emoji = "🪥" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var created = await createResponse.Content.ReadFromJsonAsync<HabitTask>();

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/tasks/{created!.Id}",
            created with { Label = "" });

        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
    }

    [Fact]
    public async Task PutTask_NullId_ReturnsBadRequest()
    {
        var task = new HabitTask { Label = "Test", Emoji = "🪥" };
        var response = await _client.PutAsJsonAsync("/api/tasks/", task);

        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_InvalidId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/tasks/invalid-id-xyz");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkComplete_EmptyTaskId_ReturnsBadRequest()
    {
        var response = await _client.PostAsync("/api/progress/complete/", null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_EmptyName_ReturnsBadRequest()
    {
        var profile = new ChildProfile { Name = "", Avatar = "👦" };
        var response = await _client.PutAsJsonAsync("/api/profile", profile);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error?.Error);
        Assert.NotNull(error!.Error.ValidationErrors);
        Assert.Contains("name", error.Error.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task UpdateProfile_EmptyAvatar_ReturnsBadRequest()
    {
        var profile = new ChildProfile { Name = "Child", Avatar = "" };
        var response = await _client.PutAsJsonAsync("/api/profile", profile);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error?.Error);
        Assert.NotNull(error!.Error.ValidationErrors);
        Assert.Contains("avatar", error.Error.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task UpdateProfile_NameTooLong_ReturnsBadRequest()
    {
        var longName = new string('a', 51);
        var profile = new ChildProfile { Name = longName, Avatar = "👦" };
        var response = await _client.PutAsJsonAsync("/api/profile", profile);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidatePin_EmptyPin_ReturnsBadRequest()
    {
        var request = new { pin = "" };
        var response = await _client.PostAsJsonAsync("/api/auth/validate-pin", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidatePin_PinTooLong_ReturnsBadRequest()
    {
        var longPin = new string('1', 21);
        var request = new { pin = longPin };
        var response = await _client.PostAsJsonAsync("/api/auth/validate-pin", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_ReturnsOkWithEmptyList()
    {
        var response = await _client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<List<HabitTask>>();
        Assert.NotNull(tasks);
    }

    [Fact]
    public async Task MarkComplete_ValidTaskId_ReturnsOk()
    {
        var task = new HabitTask { Label = "Test Task", Emoji = "🪥" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", task);
        var created = await createResponse.Content.ReadFromJsonAsync<HabitTask>();

        var response = await _client.PostAsync($"/api/progress/complete/{created!.Id}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var progress = await response.Content.ReadFromJsonAsync<DailyProgress>();
        Assert.NotNull(progress);
        Assert.Contains(created.Id, progress!.CompletedTaskIds);
    }

    [Fact]
    public async Task MarkComplete_SameTaskTwice_DoesNotDuplicate()
    {
        var task = new HabitTask { Label = "Test", Emoji = "🪥" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", task);
        var created = await createResponse.Content.ReadFromJsonAsync<HabitTask>();

        await _client.PostAsync($"/api/progress/complete/{created!.Id}", null);
        var response2 = await _client.PostAsync($"/api/progress/complete/{created.Id}", null);

        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var progress = await response2.Content.ReadFromJsonAsync<DailyProgress>();
        var count = progress!.CompletedTaskIds.Count(id => id == created.Id);
        Assert.Equal(1, count);
    }
}
