using Microsoft.EntityFrameworkCore;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.Entities;
using Sprout.DataAccess.Repositories;
using Xunit;

namespace Sprout.Api.Tests;

public class DbProfileRepositoryTests : IAsyncLifetime
{
    private readonly SproutDbContext _context;
    private readonly ProfileRepository _repository;
    private readonly DbContextOptions<SproutDbContext> _options;

    public DbProfileRepositoryTests()
    {
        var databaseName = $"test_db_{Guid.NewGuid()}";
        _options = new DbContextOptionsBuilder<SproutDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        _context = new SproutDbContext(_options);
        _repository = new ProfileRepository(_context);
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetAsync_ReturnsProfileWhenExists()
    {
        var profile = new ChildProfileEntity { Id = 1, Name = "Jeth", Avatar = "👦" };
        _context.ChildProfiles.Add(profile);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAsync();

        Assert.NotNull(result);
        Assert.Equal("Jeth", result.Name);
        Assert.Equal("👦", result.Avatar);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWhenNotExists()
    {
        var result = await _repository.GetAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_CreatesProfileWhenNotExists()
    {
        var result = await _repository.CreateOrUpdateAsync("Jeth", "👦");

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Jeth", result.Name);
        Assert.Equal("👦", result.Avatar);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_UpdatesProfileWhenExists()
    {
        var profile = new ChildProfileEntity { Id = 1, Name = "Old Name", Avatar = "🧒" };
        _context.ChildProfiles.Add(profile);
        await _context.SaveChangesAsync();

        var result = await _repository.CreateOrUpdateAsync("New Name", "👧");

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("👧", result.Avatar);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ThrowsWhenNameIsNull()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository.CreateOrUpdateAsync("", "👦"));
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ThrowsWhenAvatarIsNull()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository.CreateOrUpdateAsync("Name", ""));
    }

    [Fact]
    public async Task CreateOrUpdateAsync_PersistsProfileToDB()
    {
        await _repository.CreateOrUpdateAsync("Jeth", "👦");

        var dbProfile = await _context.ChildProfiles.FirstOrDefaultAsync(p => p.Id == 1);
        Assert.NotNull(dbProfile);
        Assert.Equal("Jeth", dbProfile.Name);
    }
}
