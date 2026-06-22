using BankingPipeline.Models;

namespace BankingPipeline.Tests.Helpers;

public static class EnvelopeFactory
{
    public static MessageEnvelope CreateValidated(
        string txnId = "TXN001",
        string amount = "1500.00",
        string currency = "USD",
        string transactionType = "transfer",
        string country = "US",
        string timestamp = "2026-03-16T09:00:00Z",
        string status = "validated")
    {
        return new MessageEnvelope
        {
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow.ToString("o"),
            SourceAgent = "transaction_validator",
            TargetAgent = "fraud_detector",
            MessageType = "transaction",
            Data = new EnvelopeData
            {
                TransactionId = txnId,
                Amount = amount,
                Currency = currency,
                TransactionType = transactionType,
                Status = status,
                SourceAccount = "ACC-1001",
                DestinationAccount = "ACC-2001",
                Description = "Test transaction",
                Timestamp = timestamp,
                Metadata = new TransactionMetadata { Channel = "online", Country = country }
            }
        };
    }

    public static MessageEnvelope CreatePending(
        string txnId = "TXN001",
        string amount = "1500.00",
        string currency = "USD",
        string transactionType = "transfer",
        string country = "US",
        string timestamp = "2026-03-16T09:00:00Z")
    {
        return new MessageEnvelope
        {
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow.ToString("o"),
            SourceAgent = "integrator",
            TargetAgent = "transaction_validator",
            MessageType = "transaction",
            Data = new EnvelopeData
            {
                TransactionId = txnId,
                Amount = amount,
                Currency = currency,
                TransactionType = transactionType,
                Status = "pending",
                SourceAccount = "ACC-1001",
                DestinationAccount = "ACC-2001",
                Description = "Test transaction",
                Timestamp = timestamp,
                Metadata = new TransactionMetadata { Channel = "online", Country = country }
            }
        };
    }
}
