using System.Text.Json;
using Sprout.Api.Models;

namespace Sprout.Api.Services;

public class JsonChildProfileService : IChildProfileService
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JsonChildProfileService(IConfiguration config)
    {
        var dataPath = config["Storage:DataPath"] ?? "Storage/data";
        _filePath = Path.Combine(dataPath, "profile.json");
    }

    public async Task<ChildProfile> GetAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new ChildProfile();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            var profile = JsonSerializer.Deserialize<ChildProfile>(json, _options);
            return profile ?? new ChildProfile();
        }
        catch
        {
            return new ChildProfile();
        }
    }

    public async Task<ChildProfile> UpdateAsync(ChildProfile profile)
    {
        await _lock.WaitAsync();
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(profile, _options);
            await File.WriteAllTextAsync(_filePath, json);
            return profile;
        }
        finally
        {
            _lock.Release();
        }
    }
}
