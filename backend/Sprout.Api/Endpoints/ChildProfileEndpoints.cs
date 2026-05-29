using Sprout.Api.Models;
using Sprout.Api.Services;

namespace Sprout.Api.Endpoints;

public static class ChildProfileEndpoints
{
    public static void MapChildProfileEndpoints(this WebApplication app)
    {
        app.MapGet("/api/profile", async (IChildProfileService svc) =>
            Results.Ok(await svc.GetAsync()));

        app.MapPut("/api/profile", async (ChildProfile profile, IChildProfileService svc) =>
            Results.Ok(await svc.UpdateAsync(profile)));
    }
}
