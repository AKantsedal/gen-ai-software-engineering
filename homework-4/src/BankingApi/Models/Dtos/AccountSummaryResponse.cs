namespace BankingApi.Models.Dtos;

public class AccountSummaryResponse
{
    public string AccountId { get; set; } = string.Empty;
    public decimal TotalDeposits { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public int TransactionCount { get; set; }
    public string? MostRecentTransactionDate { get; set; }
}
