using System.Net;
using System.Net.Http.Json;
using Sprout.Api.Models;
using Xunit;

namespace Sprout.Api.Tests;

public class ChildProfileEndpointsTests : IClassFixture<SproutWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChildProfileEndpointsTests(SproutWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProfile_Returns200WithNameAndAvatar()
    {
        var response = await _client.GetAsync("/api/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ChildProfile>();
        Assert.False(string.IsNullOrWhiteSpace(profile!.Name));
        Assert.False(string.IsNullOrWhiteSpace(profile.Avatar));
    }

    [Fact]
    public async Task PutProfile_ValidProfile_ReturnsUpdatedAndPersists()
    {
        var update = new ChildProfile { Name = "Mila", Avatar = "🐻" };

        var put = await _client.PutAsJsonAsync("/api/profile", update);

        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        var fetched = await _client.GetFromJsonAsync<ChildProfile>("/api/profile");
        Assert.Equal("Mila", fetched!.Name);
        Assert.Equal("🐻", fetched.Avatar);
    }

    [Fact]
    public async Task PutProfile_NameAtMaxLength_IsAccepted()
    {
        var response = await _client.PutAsJsonAsync("/api/profile",
            new ChildProfile { Name = new string('a', 50), Avatar = "🐻" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("", "🐻", "name")]
    [InlineData("   ", "🐻", "name")]
    [InlineData("Mila", "", "avatar")]
    [InlineData("Mila", "   ", "avatar")]
    public async Task PutProfile_BlankField_ReturnsValidationErrorForThatField(string name, string avatar, string field)
    {
        var response = await _client.PutAsJsonAsync("/api/profile", new ChildProfile { Name = name, Avatar = avatar });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("VALIDATION_ERROR", error!.Error.Code);
        Assert.Contains(field, error.Error.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task PutProfile_NameTooLong_ReturnsValidationError()
    {
        var response = await _client.PutAsJsonAsync("/api/profile",
            new ChildProfile { Name = new string('a', 51), Avatar = "🐻" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Contains("name", error!.Error.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task PutProfile_AvatarTooLong_ReturnsValidationError()
    {
        var response = await _client.PutAsJsonAsync("/api/profile",
            new ChildProfile { Name = "Mila", Avatar = new string('x', 11) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Contains("avatar", error!.Error.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task PutProfile_BothFieldsInvalid_ReportsBothErrors()
    {
        var response = await _client.PutAsJsonAsync("/api/profile", new ChildProfile { Name = "", Avatar = "" });

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal(2, error!.Error.ValidationErrors!.Count);
    }

    [Fact]
    public async Task PutProfile_InvalidRequest_DoesNotOverwriteStoredProfile()
    {
        await _client.PutAsJsonAsync("/api/profile", new ChildProfile { Name = "Keep", Avatar = "🐼" });

        await _client.PutAsJsonAsync("/api/profile", new ChildProfile { Name = "", Avatar = "🐼" });

        var fetched = await _client.GetFromJsonAsync<ChildProfile>("/api/profile");
        Assert.Equal("Keep", fetched!.Name);
    }
}
