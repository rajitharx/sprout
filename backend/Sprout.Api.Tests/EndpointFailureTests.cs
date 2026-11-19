using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sprout.Api.Models;
using Sprout.Api.Services;
using Xunit;

namespace Sprout.Api.Tests;

public class EndpointFailureTests : IClassFixture<SproutWebApplicationFactory>
{
    private readonly SproutWebApplicationFactory _factory;

    public EndpointFailureTests(SproutWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient ClientWith(Action<IServiceCollection> configure) =>
        _factory.WithWebHostBuilder(b => b.ConfigureTestServices(configure)).CreateClient();

    private static HttpClient WithFailingTasks(EndpointFailureTests t)
    {
        var svc = new Mock<ITaskService>();
        svc.Setup(s => s.GetAllAsync()).ThrowsAsync(new InvalidOperationException());
        svc.Setup(s => s.CreateAsync(It.IsAny<HabitTask>())).ThrowsAsync(new InvalidOperationException());
        svc.Setup(s => s.UpdateAsync(It.IsAny<string>(), It.IsAny<HabitTask>())).ThrowsAsync(new InvalidOperationException());
        svc.Setup(s => s.DeleteAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
        return t.ClientWith(s => s.AddScoped(_ => svc.Object));
    }

    private HttpClient WithFailingProgress()
    {
        var svc = new Mock<IProgressService>();
        svc.Setup(s => s.GetTodayAsync()).ThrowsAsync(new InvalidOperationException());
        svc.Setup(s => s.GetWeekAsync()).ThrowsAsync(new InvalidOperationException());
        svc.Setup(s => s.MarkCompleteAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
        svc.Setup(s => s.MarkIncompleteAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
        return ClientWith(s => s.AddScoped(_ => svc.Object));
    }

    private HttpClient WithFailingProfile()
    {
        var svc = new Mock<IChildProfileService>();
        svc.Setup(s => s.GetAsync()).ThrowsAsync(new InvalidOperationException());
        svc.Setup(s => s.UpdateAsync(It.IsAny<ChildProfile>())).ThrowsAsync(new InvalidOperationException());
        return ClientWith(s => s.AddScoped(_ => svc.Object));
    }

    private static readonly HabitTask ValidTask = new() { Label = "Brush", Emoji = "🪥" };

    [Fact]
    public async Task GetTasks_ServiceThrows_Returns500()
    {
        var response = await WithFailingTasks(this).GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task PostTask_ServiceThrows_Returns500()
    {
        var response = await WithFailingTasks(this).PostAsJsonAsync("/api/tasks", ValidTask);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task PutTask_ServiceThrows_Returns500()
    {
        var response = await WithFailingTasks(this).PutAsJsonAsync("/api/tasks/abc", ValidTask);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_ServiceThrows_Returns500()
    {
        var response = await WithFailingTasks(this).DeleteAsync("/api/tasks/abc");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetToday_ServiceThrows_Returns500()
    {
        var response = await WithFailingProgress().GetAsync("/api/progress/today");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetWeek_ServiceThrows_Returns500()
    {
        var response = await WithFailingProgress().GetAsync("/api/progress/week");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task MarkComplete_ServiceThrows_Returns500()
    {
        var response = await WithFailingProgress().PostAsync("/api/progress/complete/abc", null);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task MarkIncomplete_ServiceThrows_Returns500()
    {
        var response = await WithFailingProgress().PostAsync("/api/progress/incomplete/abc", null);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_ServiceThrows_Returns500()
    {
        var response = await WithFailingProfile().GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task PutProfile_ServiceThrows_Returns500()
    {
        var response = await WithFailingProfile()
            .PutAsJsonAsync("/api/profile", new ChildProfile { Name = "Mila", Avatar = "🐻" });
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Theory]
    [InlineData("PUT", "/api/tasks/%20")]
    [InlineData("DELETE", "/api/tasks/%20")]
    [InlineData("POST", "/api/progress/complete/%20")]
    [InlineData("POST", "/api/progress/incomplete/%20")]
    public async Task BlankIdInRoute_Returns400WithValidationError(string method, string url)
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "PUT")
            request.Content = JsonContent.Create(ValidTask);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("VALIDATION_ERROR", error!.Error.Code);
    }
}
