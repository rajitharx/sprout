using Microsoft.Extensions.Configuration;
using Sprout.Api.Services;
using Xunit;

namespace Sprout.Api.Tests;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task ValidatePin_WithCorrectPin_ReturnsTrue()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:ParentPin"] = "1234"
            })
            .Build();

        var service = new ConfigurationAuthenticationService(config);
        var result = await service.ValidatePinAsync("1234");

        Assert.True(result);
    }

    [Fact]
    public async Task ValidatePin_WithIncorrectPin_ReturnsFalse()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:ParentPin"] = "1234"
            })
            .Build();

        var service = new ConfigurationAuthenticationService(config);
        var result = await service.ValidatePinAsync("5678");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidatePin_WithEmptyPin_ReturnsFalse()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:ParentPin"] = "1234"
            })
            .Build();

        var service = new ConfigurationAuthenticationService(config);
        var result = await service.ValidatePinAsync("");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidatePin_UsesDefaultPin_IfNotConfigured()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var service = new ConfigurationAuthenticationService(config);
        var result = await service.ValidatePinAsync("1234");

        Assert.True(result);
    }
}
