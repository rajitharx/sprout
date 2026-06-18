using Microsoft.EntityFrameworkCore;
using Sprout.DataAccess.Entities;

namespace Sprout.DataAccess.Context;

public class SproutDbContext : DbContext
{
    public SproutDbContext(DbContextOptions<SproutDbContext> options) : base(options)
    {
    }

    public DbSet<HabitTaskEntity> HabitTasks { get; set; } = null!;
    public DbSet<DailyProgressEntity> DailyProgress { get; set; } = null!;
    public DbSet<CompletedTaskEntity> CompletedTasks { get; set; } = null!;
    public DbSet<ChildProfileEntity> ChildProfiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HabitTaskEntity>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<ChildProfileEntity>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<DailyProgressEntity>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<DailyProgressEntity>()
            .HasMany(p => p.CompletedTasks)
            .WithOne(c => c.DailyProgress)
            .HasForeignKey(c => c.DailyProgressId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CompletedTaskEntity>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<CompletedTaskEntity>()
            .HasIndex(c => new { c.DailyProgressId, c.TaskId })
            .IsUnique();

        modelBuilder.Entity<DailyProgressEntity>()
            .HasIndex(p => p.Date)
            .IsUnique();
    }
}
