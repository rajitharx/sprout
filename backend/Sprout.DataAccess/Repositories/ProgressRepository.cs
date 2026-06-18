using Microsoft.EntityFrameworkCore;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.Entities;

namespace Sprout.DataAccess.Repositories;

public class ProgressRepository : IProgressRepository
{
    private readonly SproutDbContext _context;

    public ProgressRepository(SproutDbContext context)
    {
        _context = context;
    }

    public async Task<DailyProgressEntity?> GetByDateAsync(string date)
    {
        return await _context.DailyProgress
            .Include(p => p.CompletedTasks)
            .FirstOrDefaultAsync(p => p.Date == date);
    }

    public async Task<List<DailyProgressEntity>> GetWeekAsync(string startDate, string endDate)
    {
        return await _context.DailyProgress
            .Include(p => p.CompletedTasks)
            .Where(p => string.Compare(p.Date, startDate) >= 0 && string.Compare(p.Date, endDate) <= 0)
            .OrderBy(p => p.Date)
            .ToListAsync();
    }

    public async Task<DailyProgressEntity> CreateOrUpdateAsync(string date, List<string> completedTaskIds)
    {
        var progress = await _context.DailyProgress
            .Include(p => p.CompletedTasks)
            .FirstOrDefaultAsync(p => p.Date == date);

        if (progress == null)
        {
            progress = new DailyProgressEntity
            {
                Date = date,
                LastUpdated = DateTime.UtcNow,
                CompletedTasks = completedTaskIds
                    .Select(tid => new CompletedTaskEntity { TaskId = tid })
                    .ToList()
            };
            _context.DailyProgress.Add(progress);
        }
        else
        {
            progress.CompletedTasks = completedTaskIds
                .Select(tid => new CompletedTaskEntity { TaskId = tid, DailyProgressId = progress.Id })
                .ToList();
            progress.LastUpdated = DateTime.UtcNow;
            _context.DailyProgress.Update(progress);
        }

        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task<DailyProgressEntity> MarkCompleteAsync(string date, string taskId)
    {
        var progress = await _context.DailyProgress
            .Include(p => p.CompletedTasks)
            .FirstOrDefaultAsync(p => p.Date == date);

        if (progress == null)
        {
            progress = new DailyProgressEntity
            {
                Date = date,
                LastUpdated = DateTime.UtcNow,
                CompletedTasks = [new CompletedTaskEntity { TaskId = taskId }]
            };
            _context.DailyProgress.Add(progress);
        }
        else if (!progress.CompletedTasks.Any(c => c.TaskId == taskId))
        {
            progress.CompletedTasks.Add(new CompletedTaskEntity { TaskId = taskId });
            progress.LastUpdated = DateTime.UtcNow;
            _context.DailyProgress.Update(progress);
        }

        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task<DailyProgressEntity> MarkIncompleteAsync(string date, string taskId)
    {
        var progress = await _context.DailyProgress
            .Include(p => p.CompletedTasks)
            .FirstOrDefaultAsync(p => p.Date == date);

        if (progress != null)
        {
            var completed = progress.CompletedTasks.FirstOrDefault(c => c.TaskId == taskId);
            if (completed != null)
            {
                progress.CompletedTasks.Remove(completed);
                progress.LastUpdated = DateTime.UtcNow;
                _context.DailyProgress.Update(progress);
                await _context.SaveChangesAsync();
            }
        }

        return progress ?? new DailyProgressEntity { Date = date, LastUpdated = DateTime.UtcNow };
    }
}
