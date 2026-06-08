using Sprout.Api.Models;
using Sprout.Api.Services;

namespace Sprout.Api.Endpoints;

public static class ChildProfileEndpoints
{
    public static void MapChildProfileEndpoints(this WebApplication app)
    {
        app.MapGet("/api/profile", async (IChildProfileService svc) =>
        {
            try
            {
                var profile = await svc.GetAsync();
                return Results.Ok(profile);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });

        app.MapPut("/api/profile", async (ChildProfile profile, IChildProfileService svc) =>
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                errors["name"] = ["Child name is required."];
            }
            else if (profile.Name.Length > 50)
            {
                errors["name"] = ["Child name must not exceed 50 characters."];
            }

            if (string.IsNullOrWhiteSpace(profile.Avatar))
            {
                errors["avatar"] = ["Avatar emoji is required."];
            }
            else if (profile.Avatar.Length > 10)
            {
                errors["avatar"] = ["Avatar emoji is too long."];
            }

            if (errors.Count > 0)
            {
                return Results.BadRequest(ApiErrors.ValidationError(errors));
            }

            try
            {
                var updated = await svc.UpdateAsync(profile);
                return Results.Ok(updated);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });
    }
}
