using System.Text.Json.Serialization;

namespace BankingPipeline.Models;

public class RawTransaction
{
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("source_account")]
    public string SourceAccount { get; set; } = string.Empty;

    [JsonPropertyName("destination_account")]
    public string DestinationAccount { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("transaction_type")]
    public string TransactionType { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public TransactionMetadata Metadata { get; set; } = new();
}

public class TransactionMetadata
{
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}
