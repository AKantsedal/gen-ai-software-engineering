using System.Globalization;
using System.Text.Json;
using BankingPipeline.Helpers;
using BankingPipeline.Models;
using Microsoft.Extensions.Logging;

namespace BankingPipeline.Agents;

public class TransactionValidator
{
    private readonly ILogger<TransactionValidator> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly HashSet<string> ValidCurrencies = new(StringComparer.Ordinal)
    {
        "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY",
        "SEK", "NOK", "DKK", "SGD", "HKD", "NZD", "MXN", "BRL",
        "INR", "ZAR", "KRW"
    };

    public TransactionValidator(ILoggerFactory loggerFactory, JsonSerializerOptions jsonOptions)
    {
        _logger = loggerFactory.CreateLogger<TransactionValidator>();
        _jsonOptions = jsonOptions;
    }

    public async Task<string> ProcessAsync(string envelopeFilePath, CancellationToken ct = default)
    {
        var envelope = await FileHelper.ReadJsonAsync<MessageEnvelope>(envelopeFilePath, _jsonOptions, ct);
        var data = envelope.Data;
        var txnId = data.TransactionId;

        // Check required fields
        string? rejectionReason = ValidateRequiredFields(data);

        // Parse and validate amount
        if (rejectionReason is null)
        {
            if (!decimal.TryParse(data.Amount, CultureInfo.InvariantCulture, out var amount))
            {
                rejectionReason = "invalid_amount";
            }
            else if (amount < 0 && !string.Equals(data.TransactionType, "refund", StringComparison.OrdinalIgnoreCase))
            {
                rejectionReason = "negative_amount";
            }
        }

        // Validate currency
        if (rejectionReason is null && !ValidCurrencies.Contains(data.Currency))
        {
            rejectionReason = "invalid_currency_code";
        }

        if (rejectionReason is not null)
        {
            return await RejectAsync(envelope, rejectionReason, envelopeFilePath, ct);
        }

        return await ValidateAsync(envelope, envelopeFilePath, ct);
    }

    private static string? ValidateRequiredFields(EnvelopeData data)
    {
        if (string.IsNullOrEmpty(data.TransactionId) ||
            string.IsNullOrEmpty(data.Amount) ||
            string.IsNullOrEmpty(data.Currency) ||
            string.IsNullOrEmpty(data.TransactionType) ||
            string.IsNullOrEmpty(data.SourceAccount) ||
            string.IsNullOrEmpty(data.DestinationAccount) ||
            string.IsNullOrEmpty(data.Timestamp))
        {
            return "missing_required_field";
        }

        return null;
    }

    private async Task<string> ValidateAsync(
        MessageEnvelope envelope, string sourceFilePath, CancellationToken ct)
    {
        envelope.SourceAgent = "transaction_validator";
        envelope.TargetAgent = "fraud_detector";
        envelope.Timestamp = DateTime.UtcNow.ToString("o");
        envelope.Data.Status = "validated";

        var outputPath = Path.Combine("shared/output", $"{envelope.Data.TransactionId}.json");
        await FileHelper.WriteJsonAtomicAsync(outputPath, envelope, _jsonOptions, ct);
        File.Delete(sourceFilePath);

        _logger.LogInformation(
            "Transaction {TransactionId} validated ({Source} → {Destination})",
            envelope.Data.TransactionId,
            PiiMasker.MaskAccount(envelope.Data.SourceAccount),
            PiiMasker.MaskAccount(envelope.Data.DestinationAccount));

        return "validated";
    }

    private async Task<string> RejectAsync(
        MessageEnvelope envelope, string reason, string sourceFilePath, CancellationToken ct)
    {
        envelope.SourceAgent = "transaction_validator";
        envelope.TargetAgent = "none";
        envelope.Timestamp = DateTime.UtcNow.ToString("o");
        envelope.Data.Status = "rejected";
        envelope.Data.RejectionReason = reason;

        var fileKey = string.IsNullOrEmpty(envelope.Data.TransactionId)
            ? Path.GetFileNameWithoutExtension(sourceFilePath)
            : envelope.Data.TransactionId;
        var resultPath = Path.Combine("shared/results", $"{fileKey}.json");
        await FileHelper.WriteJsonAtomicAsync(resultPath, envelope, _jsonOptions, ct);
        File.Delete(sourceFilePath);

        _logger.LogWarning(
            "Transaction {TransactionId} rejected: {Reason} ({Source} → {Destination})",
            envelope.Data.TransactionId,
            reason,
            PiiMasker.MaskAccount(envelope.Data.SourceAccount),
            PiiMasker.MaskAccount(envelope.Data.DestinationAccount));

        return "rejected";
    }
}
