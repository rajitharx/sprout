using System.Text.Json;
using Sprout.Api.Models;

namespace Sprout.Api.Services;

public class JsonChildProfileService : IChildProfileService
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<JsonChildProfileService> _logger;
    private readonly bool _logServiceCalls;
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JsonChildProfileService(IConfiguration config, ILogger<JsonChildProfileService> logger)
    {
        _logger = logger;
        _logServiceCalls = config.GetSection("Debug").GetValue<bool>("LogServiceCalls");
        var dataPath = config["Storage:DataPath"] ?? "Storage/data";
        _filePath = Path.Combine(dataPath, "profile.json");
    }

    public async Task<ChildProfile> GetAsync()
    {
        if (!File.Exists(_filePath))
        {
            if (_logServiceCalls) _logger.LogDebug("👶 GetAsync no profile found, returning default");
            return new ChildProfile();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            var profile = JsonSerializer.Deserialize<ChildProfile>(json, _options);
            var result = profile ?? new ChildProfile();
            if (_logServiceCalls) _logger.LogDebug("👶 GetAsync loaded profile: {Name} {Avatar}", result.Name, result.Avatar);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GetAsync failed to deserialize profile");
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
            _logger.LogInformation("👶 Child profile updated: {Name} {Avatar}", profile.Name, profile.Avatar);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ UpdateAsync failed to save profile");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
}
