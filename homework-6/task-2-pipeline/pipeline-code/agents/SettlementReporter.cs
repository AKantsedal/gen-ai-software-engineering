using System.Text.Json;
using BankingPipeline.Helpers;
using BankingPipeline.Models;
using Microsoft.Extensions.Logging;

namespace BankingPipeline.Agents;

public class SettlementReporter
{
    private readonly ILogger<SettlementReporter> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SettlementReporter(ILoggerFactory loggerFactory, JsonSerializerOptions jsonOptions)
    {
        _logger = loggerFactory.CreateLogger<SettlementReporter>();
        _jsonOptions = jsonOptions;
    }

    public async Task<string> ProcessAsync(string envelopeFilePath, CancellationToken ct = default)
    {
        var envelope = await FileHelper.ReadJsonAsync<MessageEnvelope>(envelopeFilePath, _jsonOptions, ct);
        var data = envelope.Data;

        var settlementStatus = data.Status switch
        {
            "approved" => "settled",
            "flagged" => "held_for_review",
            _ => throw new InvalidOperationException(
                $"Unexpected status '{data.Status}' for transaction {data.TransactionId}")
        };

        envelope.SourceAgent = "settlement_reporter";
        envelope.TargetAgent = "none";
        envelope.Timestamp = DateTime.UtcNow.ToString("o");
        envelope.Data.SettlementStatus = settlementStatus;

        var resultPath = Path.Combine("shared/results", $"{data.TransactionId}.json");
        await FileHelper.WriteJsonAtomicAsync(resultPath, envelope, _jsonOptions, ct);
        File.Delete(envelopeFilePath);

        if (settlementStatus == "held_for_review")
        {
            _logger.LogWarning(
                "Transaction {TransactionId} — {SettlementStatus} ({Source} → {Destination})",
                data.TransactionId, settlementStatus,
                PiiMasker.MaskAccount(data.SourceAccount),
                PiiMasker.MaskAccount(data.DestinationAccount));
        }
        else
        {
            _logger.LogInformation(
                "Transaction {TransactionId} — {SettlementStatus} ({Source} → {Destination})",
                data.TransactionId, settlementStatus,
                PiiMasker.MaskAccount(data.SourceAccount),
                PiiMasker.MaskAccount(data.DestinationAccount));
        }

        return settlementStatus;
    }
}
