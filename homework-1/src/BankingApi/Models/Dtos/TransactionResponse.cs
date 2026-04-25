namespace BankingApi.Models.Dtos;

public class TransactionResponse
{
    public string Id { get; set; } = string.Empty;
    public string FromAccount { get; set; } = string.Empty;
    public string ToAccount { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public static TransactionResponse FromTransaction(Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            FromAccount = transaction.FromAccount,
            ToAccount = transaction.ToAccount,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Type = transaction.Type.ToString().ToLower(),
            Timestamp = transaction.Timestamp.ToString("o"),
            Status = transaction.Status.ToString().ToLower()
        };
    }
}
