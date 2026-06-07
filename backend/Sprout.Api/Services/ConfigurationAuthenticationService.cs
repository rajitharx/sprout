namespace Sprout.Api.Services;

public class ConfigurationAuthenticationService : IAuthenticationService
{
    private readonly string _parentPin;

    public ConfigurationAuthenticationService(IConfiguration configuration)
    {
        _parentPin = configuration.GetSection("Authentication").GetValue<string>("ParentPin") ?? "1234";
    }

    public Task<bool> ValidatePinAsync(string pin)
    {
        return Task.FromResult(pin == _parentPin);
    }
}
