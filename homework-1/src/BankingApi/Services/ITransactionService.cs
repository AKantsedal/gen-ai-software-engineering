using BankingApi.Models;

namespace BankingApi.Services;

public interface ITransactionService
{
    Transaction Create(string fromAccount, string toAccount, decimal amount, string currency, TransactionType type);
    IEnumerable<Transaction> GetAll();
    Transaction? GetById(string id);
    decimal GetBalance(string accountId);
    Models.Dtos.AccountSummaryResponse GetSummary(string accountId);
}
