namespace BankingApi.Models;

public class Transaction
{
    public string Id { get; set; } = string.Empty;
    public string FromAccount { get; set; } = string.Empty;
    public string ToAccount { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public TransactionStatus Status { get; set; }
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed
}
