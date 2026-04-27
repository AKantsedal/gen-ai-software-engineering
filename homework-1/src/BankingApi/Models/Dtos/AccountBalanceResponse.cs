using BankingApi.Models;

namespace BankingApi.Models.Dtos;

public class AccountBalanceResponse
{
    public string AccountId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public Currency Currency { get; set; }
}
