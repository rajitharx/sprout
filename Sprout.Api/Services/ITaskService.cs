using Sprout.Api.Models;

namespace Sprout.Api.Services;

public interface ITaskService
{
    Task<List<HabitTask>> GetAllAsync();
    Task<HabitTask?> GetByIdAsync(string id);
    Task<HabitTask> CreateAsync(HabitTask task);
    Task<HabitTask?> UpdateAsync(string id, HabitTask task);
    Task<bool> DeleteAsync(string id);
}
