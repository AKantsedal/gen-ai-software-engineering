namespace BankingApi.Models.Dtos;

public class AccountBalanceResponse
{
    public string AccountId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
}
