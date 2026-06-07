namespace Sprout.Api.Services;

public interface IAuthenticationService
{
    Task<bool> ValidatePinAsync(string pin);
}
