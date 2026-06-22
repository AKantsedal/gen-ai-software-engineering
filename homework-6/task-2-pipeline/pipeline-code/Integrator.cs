using System.Text.Json;
using BankingPipeline.Agents;
using BankingPipeline.Helpers;
using BankingPipeline.Models;
using Microsoft.Extensions.Logging;

namespace BankingPipeline;

public class Integrator
{
    private readonly ILogger<Integrator> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly string[] SharedDirs =
        ["shared/input", "shared/processing", "shared/output", "shared/results"];

    public Integrator(ILoggerFactory loggerFactory, JsonSerializerOptions jsonOptions)
    {
        _loggerFactory = loggerFactory;
        _jsonOptions = jsonOptions;
        _logger = loggerFactory.CreateLogger<Integrator>();
    }

    public async Task RunAsync(string transactionsFilePath, CancellationToken ct = default)
    {
        _logger.LogInformation("Pipeline started at {Timestamp}", DateTime.UtcNow.ToString("o"));

        EnsureDirectories();
        ClearDirectories();

        var transactions = await LoadTransactionsAsync(transactionsFilePath, ct);
        _logger.LogInformation("Loaded {Count} transactions from {File}", transactions.Count, transactionsFilePath);

        await CreateEnvelopesAsync(transactions, ct);
        await MoveFilesAsync("shared/input", "shared/processing", ct);

        // Stage 1: Validation
        var validator = new TransactionValidator(_loggerFactory, _jsonOptions);
        await ProcessFilesInDirectoryAsync("shared/processing", validator.ProcessAsync, ct);

        // Stage 2: Fraud Detection
        var fraudDetector = new FraudDetector(_loggerFactory, _jsonOptions);
        await ProcessFilesInDirectoryAsync("shared/output", fraudDetector.ProcessAsync, ct);

        // Stage 3: Settlement Reporting
        var settlementReporter = new SettlementReporter(_loggerFactory, _jsonOptions);
        await ProcessFilesInDirectoryAsync("shared/processing", settlementReporter.ProcessAsync, ct);

        // Generate summary
        await GenerateSummaryAsync(ct);

        _logger.LogInformation("Pipeline completed at {Timestamp}", DateTime.UtcNow.ToString("o"));
    }

    private void EnsureDirectories()
    {
        foreach (var dir in SharedDirs)
            Directory.CreateDirectory(dir);
    }

    private void ClearDirectories()
    {
        foreach (var dir in SharedDirs)
        {
            foreach (var file in Directory.GetFiles(dir, "*.json"))
                File.Delete(file);
        }
    }

    private async Task<List<RawTransaction>> LoadTransactionsAsync(string filePath, CancellationToken ct)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<List<RawTransaction>>(stream, _jsonOptions, ct)
               ?? throw new InvalidOperationException("Failed to deserialize transactions file");
    }

    private async Task CreateEnvelopesAsync(List<RawTransaction> transactions, CancellationToken ct)
    {
        foreach (var txn in transactions)
        {
            var envelope = new MessageEnvelope
            {
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow.ToString("o"),
                SourceAgent = "integrator",
                TargetAgent = "transaction_validator",
                MessageType = "transaction",
                Data = new EnvelopeData
                {
                    TransactionId = txn.TransactionId,
                    Amount = txn.Amount,
                    Currency = txn.Currency,
                    TransactionType = txn.TransactionType,
                    Status = "pending",
                    SourceAccount = txn.SourceAccount,
                    DestinationAccount = txn.DestinationAccount,
                    Description = txn.Description,
                    Timestamp = txn.Timestamp,
                    Metadata = txn.Metadata
                }
            };

            var filePath = Path.Combine("shared/input", $"{txn.TransactionId}.json");
            await FileHelper.WriteJsonAtomicAsync(filePath, envelope, _jsonOptions, ct);

            _logger.LogInformation(
                "Envelope created for {TransactionId} ({Source} → {Destination})",
                txn.TransactionId,
                PiiMasker.MaskAccount(txn.SourceAccount),
                PiiMasker.MaskAccount(txn.DestinationAccount));
        }
    }

    private static async Task MoveFilesAsync(string sourceDir, string targetDir, CancellationToken ct)
    {
        foreach (var file in Directory.GetFiles(sourceDir, "*.json"))
        {
            var destPath = Path.Combine(targetDir, Path.GetFileName(file));
            File.Move(file, destPath, overwrite: true);
        }

        await Task.CompletedTask;
    }

    private static async Task ProcessFilesInDirectoryAsync(
        string directory, Func<string, CancellationToken, Task<string>> processAsync, CancellationToken ct)
    {
        var files = Directory.GetFiles(directory, "*.json");
        foreach (var file in files)
        {
            await processAsync(file, ct);
        }
    }

    private async Task GenerateSummaryAsync(CancellationToken ct)
    {
        var resultFiles = Directory.GetFiles("shared/results", "*.json")
            .Where(f => !Path.GetFileName(f).Equals("summary.json", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var summary = new PipelineSummary
        {
            Total = resultFiles.Count,
            PipelineRunTimestamp = DateTime.UtcNow.ToString("o")
        };

        foreach (var file in resultFiles)
        {
            var envelope = await FileHelper.ReadJsonAsync<MessageEnvelope>(file, _jsonOptions, ct);
            var status = envelope.Data.Status;
            var settlement = envelope.Data.SettlementStatus;

            switch (status)
            {
                case "rejected":
                    summary.Rejected++;
                    break;
                case "flagged":
                    summary.Flagged++;
                    break;
                case "approved":
                    summary.Approved++;
                    break;
            }

            switch (settlement)
            {
                case "settled":
                    summary.Settled++;
                    summary.Validated++;
                    break;
                case "held_for_review":
                    summary.HeldForReview++;
                    summary.Validated++;
                    break;
            }

            if (status == "rejected")
            {
                _logger.LogWarning(
                    "Result: {TransactionId} — REJECTED ({Reason})",
                    envelope.Data.TransactionId,
                    envelope.Data.RejectionReason);
            }
            else
            {
                _logger.LogInformation(
                    "Result: {TransactionId} — {Status}, settlement: {Settlement}",
                    envelope.Data.TransactionId,
                    status,
                    settlement ?? "n/a");
            }
        }

        var summaryPath = Path.Combine("shared/results", "summary.json");
        await FileHelper.WriteJsonAtomicAsync(summaryPath, summary, _jsonOptions, ct);

        _logger.LogInformation(
            "Summary: total={Total}, rejected={Rejected}, flagged={Flagged}, approved={Approved}, settled={Settled}, held={Held}",
            summary.Total, summary.Rejected, summary.Flagged, summary.Approved, summary.Settled, summary.HeldForReview);
    }
}
