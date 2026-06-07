using Sprout.Api.Endpoints;
using Xunit;

namespace Sprout.Api.Tests;

public class AuthenticationEndpointsTests : IClassFixture<SproutWebApplicationFactory>
{
    private readonly SproutWebApplicationFactory _factory;

    public AuthenticationEndpointsTests(SproutWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ValidatePin_WithCorrectPin_ReturnsOkWithValidTrue()
    {
        var client = _factory.CreateClient();
        var request = new PinValidationRequest("1234");
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync("/api/auth/validate-pin", content);

        Assert.True(response.IsSuccessStatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<PinValidationResponse>(
            json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        Assert.NotNull(result);
        Assert.True(result.Valid);
    }

    [Fact]
    public async Task ValidatePin_WithIncorrectPin_ReturnsOkWithValidFalse()
    {
        var client = _factory.CreateClient();
        var request = new PinValidationRequest("9999");
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync("/api/auth/validate-pin", content);

        Assert.True(response.IsSuccessStatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<PinValidationResponse>(
            json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        Assert.NotNull(result);
        Assert.False(result.Valid);
    }
}
