using Sprout.Api.Models;

namespace Sprout.Api.Services;

public interface IChildProfileService
{
    Task<ChildProfile> GetAsync();
    Task<ChildProfile> UpdateAsync(ChildProfile profile);
}
