using System.Text.Json.Serialization;

namespace BankingPipeline.Models;

public class MessageEnvelope
{
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("source_agent")]
    public string SourceAgent { get; set; } = string.Empty;

    [JsonPropertyName("target_agent")]
    public string TargetAgent { get; set; } = string.Empty;

    [JsonPropertyName("message_type")]
    public string MessageType { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public EnvelopeData Data { get; set; } = new();
}

public class EnvelopeData
{
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("transaction_type")]
    public string TransactionType { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("risk_score")]
    public int? RiskScore { get; set; }

    [JsonPropertyName("rejection_reason")]
    public string? RejectionReason { get; set; }

    [JsonPropertyName("settlement_status")]
    public string? SettlementStatus { get; set; }

    [JsonPropertyName("source_account")]
    public string SourceAccount { get; set; } = string.Empty;

    [JsonPropertyName("destination_account")]
    public string DestinationAccount { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public TransactionMetadata Metadata { get; set; } = new();
}
