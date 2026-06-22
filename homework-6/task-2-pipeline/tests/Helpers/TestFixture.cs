using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BankingPipeline.Tests.Helpers;

public static class TestFixture
{
    public static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static ILoggerFactory LoggerFactory =>
        Microsoft.Extensions.Logging.LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));

    public static (string root, string input, string processing, string output, string results) CreateTempDirs()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var input = Path.Combine(root, "shared", "input");
        var processing = Path.Combine(root, "shared", "processing");
        var output = Path.Combine(root, "shared", "output");
        var results = Path.Combine(root, "shared", "results");

        Directory.CreateDirectory(input);
        Directory.CreateDirectory(processing);
        Directory.CreateDirectory(output);
        Directory.CreateDirectory(results);

        return (root, input, processing, output, results);
    }
}
