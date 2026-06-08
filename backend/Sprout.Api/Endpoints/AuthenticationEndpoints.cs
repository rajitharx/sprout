using Sprout.Api.Models;
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
            if (string.IsNullOrWhiteSpace(request.Pin))
            {
                return Results.BadRequest(ApiErrors.ValidationError("pin", "PIN is required."));
            }

            if (request.Pin.Length > 20)
            {
                return Results.BadRequest(ApiErrors.ValidationError("pin", "PIN is too long."));
            }

            try
            {
                var valid = await svc.ValidatePinAsync(request.Pin);
                return Results.Ok(new PinValidationResponse(valid));
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });
    }
}
