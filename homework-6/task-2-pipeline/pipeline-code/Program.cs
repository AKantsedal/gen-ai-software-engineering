using System.Text.Json;
using BankingPipeline;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger("Program");

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
};

try
{
    var integrator = new Integrator(loggerFactory, jsonOptions);
    await integrator.RunAsync("sample-transactions.json");
    logger.LogInformation("Pipeline finished successfully");
    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Pipeline failed");
    return 1;
}
