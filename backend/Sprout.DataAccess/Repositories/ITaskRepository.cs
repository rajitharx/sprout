using Sprout.DataAccess.Entities;

namespace Sprout.DataAccess.Repositories;

public interface ITaskRepository
{
    Task<List<HabitTaskEntity>> GetAllActiveAsync();
    Task<HabitTaskEntity?> GetByIdAsync(string id);
    Task<HabitTaskEntity> CreateAsync(HabitTaskEntity task);
    Task<HabitTaskEntity?> UpdateAsync(string id, HabitTaskEntity task);
    Task<bool> DeleteAsync(string id);
}
