using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sprout.Api.Services;

namespace Sprout.Api.Tests;

public class SproutWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "sprout-tests-" + Guid.NewGuid().ToString("N"));
    private readonly TestSystemClock _testClock = new(new DateOnly(2026, 5, 30));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:DataPath"] = _tempDir
            });
        });
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<ISystemClock>(_testClock);
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        base.Dispose(disposing);
    }
}
