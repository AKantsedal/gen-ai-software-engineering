using System.Globalization;
using System.Text.Json;
using BankingPipeline.Helpers;
using BankingPipeline.Models;
using Microsoft.Extensions.Logging;

namespace BankingPipeline.Agents;

public class FraudDetector
{
    private readonly ILogger<FraudDetector> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FraudDetector(ILoggerFactory loggerFactory, JsonSerializerOptions jsonOptions)
    {
        _logger = loggerFactory.CreateLogger<FraudDetector>();
        _jsonOptions = jsonOptions;
    }

    public async Task<string> ProcessAsync(string envelopeFilePath, CancellationToken ct = default)
    {
        var envelope = await FileHelper.ReadJsonAsync<MessageEnvelope>(envelopeFilePath, _jsonOptions, ct);
        var data = envelope.Data;

        var amount = decimal.Parse(data.Amount, CultureInfo.InvariantCulture);
        var riskScore = ComputeRiskScore(amount, data.Timestamp, data.Metadata.Country);
        var status = riskScore >= 50 ? "flagged" : "approved";

        envelope.SourceAgent = "fraud_detector";
        envelope.TargetAgent = "settlement_reporter";
        envelope.Timestamp = DateTime.UtcNow.ToString("o");
        envelope.Data.Status = status;
        envelope.Data.RiskScore = riskScore;

        var outputPath = Path.Combine("shared/processing", $"{data.TransactionId}.json");
        await FileHelper.WriteJsonAtomicAsync(outputPath, envelope, _jsonOptions, ct);
        File.Delete(envelopeFilePath);

        if (status == "flagged")
        {
            _logger.LogWarning(
                "Transaction {TransactionId} scored {RiskScore} — {Status} ({Source} → {Destination})",
                data.TransactionId, riskScore, status,
                PiiMasker.MaskAccount(data.SourceAccount),
                PiiMasker.MaskAccount(data.DestinationAccount));
        }
        else
        {
            _logger.LogInformation(
                "Transaction {TransactionId} scored {RiskScore} — {Status} ({Source} → {Destination})",
                data.TransactionId, riskScore, status,
                PiiMasker.MaskAccount(data.SourceAccount),
                PiiMasker.MaskAccount(data.DestinationAccount));
        }

        return status;
    }

    private static int ComputeRiskScore(decimal amount, string timestamp, string country)
    {
        int score = 0;

        // High-value signal (mutually exclusive with structuring)
        if (amount > 10_000m)
        {
            score = 60;
        }
        else if (amount >= 9_000m && amount <= 9_999.99m)
        {
            score = 40; // Structuring pattern
        }

        // Off-hours signal: 00:00–05:59 UTC
        if (DateTime.TryParse(timestamp, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt))
        {
            if (dt.Hour >= 0 && dt.Hour < 6)
            {
                score += 20;
            }
        }

        // Cross-border signal
        if (!string.Equals(country, "US", StringComparison.Ordinal))
        {
            score += 15;
        }

        // Cap at 100
        return Math.Min(score, 100);
    }
}
