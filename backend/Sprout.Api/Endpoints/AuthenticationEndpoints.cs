using Sprout.Api.Services;

namespace Sprout.Api.Endpoints;

public record PinValidationRequest(string Pin);
public record PinValidationResponse(bool Valid);

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/validate-pin", async (PinValidationRequest request, IAuthenticationService svc) =>
        {
            var valid = await svc.ValidatePinAsync(request.Pin);
            return Results.Ok(new PinValidationResponse(valid));
        });
    }
}
