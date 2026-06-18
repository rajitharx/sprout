using Sprout.Api.Models;
using Sprout.DataAccess.Repositories;

namespace Sprout.Api.Services;

public class DbChildProfileService : IChildProfileService
{
    private readonly IProfileRepository _repository;
    private readonly ILogger<DbChildProfileService> _logger;
    private readonly bool _logServiceCalls;

    public DbChildProfileService(IProfileRepository repository, ILogger<DbChildProfileService> logger, IConfiguration config)
    {
        _repository = repository;
        _logger = logger;
        _logServiceCalls = config.GetSection("Debug").GetValue<bool>("LogServiceCalls");
    }

    public async Task<ChildProfile> GetAsync()
    {
        try
        {
            var profile = await _repository.GetAsync();
            var result = profile != null
                ? new ChildProfile { Name = profile.Name, Avatar = profile.Avatar }
                : new ChildProfile();

            if (_logServiceCalls) _logger.LogDebug("👶 GetAsync loaded profile: {Name} {Avatar}", result.Name, result.Avatar);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GetAsync failed to load profile");
            return new ChildProfile();
        }
    }

    public async Task<ChildProfile> UpdateAsync(ChildProfile profile)
    {
        try
        {
            var entity = await _repository.CreateOrUpdateAsync(profile.Name, profile.Avatar);
            _logger.LogInformation("👶 Child profile updated: {Name} {Avatar}", entity.Name, entity.Avatar);

            return new ChildProfile { Name = entity.Name, Avatar = entity.Avatar };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ UpdateAsync failed to save profile");
            throw;
        }
    }
}
