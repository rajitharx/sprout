using Microsoft.EntityFrameworkCore;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.Entities;

namespace Sprout.DataAccess.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly SproutDbContext _context;

    public TaskRepository(SproutDbContext context)
    {
        _context = context;
    }

    public async Task<List<HabitTaskEntity>> GetAllActiveAsync()
    {
        return await _context.HabitTasks
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<HabitTaskEntity?> GetByIdAsync(string id)
    {
        return await _context.HabitTasks.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<HabitTaskEntity> CreateAsync(HabitTaskEntity task)
    {
        if (string.IsNullOrWhiteSpace(task.Label))
            throw new ArgumentException("Task label cannot be null or empty.", nameof(task));
        if (string.IsNullOrWhiteSpace(task.Emoji))
            throw new ArgumentException("Task emoji cannot be null or empty.", nameof(task));

        task.Id = string.IsNullOrEmpty(task.Id) ? Guid.NewGuid().ToString() : task.Id;
        _context.HabitTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<HabitTaskEntity?> UpdateAsync(string id, HabitTaskEntity task)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Task ID cannot be null or empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(task.Label))
            throw new ArgumentException("Task label cannot be null or empty.", nameof(task));
        if (string.IsNullOrWhiteSpace(task.Emoji))
            throw new ArgumentException("Task emoji cannot be null or empty.", nameof(task));

        var existing = await _context.HabitTasks.FirstOrDefaultAsync(t => t.Id == id);
        if (existing == null)
            return null;

        existing.Label = task.Label;
        existing.Emoji = task.Emoji;
        existing.SortOrder = task.SortOrder;
        existing.IsActive = task.IsActive;

        _context.HabitTasks.Update(existing);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var task = await _context.HabitTasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
            return false;

        task.IsActive = false;
        _context.HabitTasks.Update(task);
        await _context.SaveChangesAsync();
        return true;
    }
}
