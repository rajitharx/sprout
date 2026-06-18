using Sprout.DataAccess.Entities;

namespace Sprout.DataAccess.Repositories;

public interface IProfileRepository
{
    Task<ChildProfileEntity?> GetAsync();
    Task<ChildProfileEntity> CreateOrUpdateAsync(string name, string avatar);
}
