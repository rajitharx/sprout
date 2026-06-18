using Microsoft.EntityFrameworkCore;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.Entities;

namespace Sprout.DataAccess.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly SproutDbContext _context;

    public ProfileRepository(SproutDbContext context)
    {
        _context = context;
    }

    public async Task<ChildProfileEntity?> GetAsync()
    {
        return await _context.ChildProfiles.FirstOrDefaultAsync(p => p.Id == 1);
    }

    public async Task<ChildProfileEntity> CreateOrUpdateAsync(string name, string avatar)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Profile name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(avatar))
            throw new ArgumentException("Profile avatar cannot be null or empty.", nameof(avatar));

        var profile = await _context.ChildProfiles.FirstOrDefaultAsync(p => p.Id == 1);

        if (profile == null)
        {
            profile = new ChildProfileEntity { Id = 1, Name = name, Avatar = avatar };
            _context.ChildProfiles.Add(profile);
        }
        else
        {
            profile.Name = name;
            profile.Avatar = avatar;
            _context.ChildProfiles.Update(profile);
        }

        await _context.SaveChangesAsync();
        return profile;
    }
}
