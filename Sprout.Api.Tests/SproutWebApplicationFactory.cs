using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Sprout.Api.Tests;

public class SproutWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "sprout-tests-" + Guid.NewGuid().ToString("N"));

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
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        base.Dispose(disposing);
    }
}
