using BankingApi.Models;
using BankingApi.Models.Dtos;
using BankingApi.Repositories;

namespace BankingApi.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;

    public TransactionService(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public Transaction Create(string fromAccount, string toAccount, decimal amount, string currency, TransactionType type)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            FromAccount = fromAccount,
            ToAccount = toAccount,
            Amount = amount,
            Currency = currency,
            Type = type,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        };

        _repository.Add(transaction);
        return transaction;
    }

    public IEnumerable<Transaction> GetAll()
    {
        return _repository.GetAll();
    }

    public Transaction? GetById(string id)
    {
        return _repository.GetById(id);
    }

    public decimal GetBalance(string accountId)
    {
        var transactions = _repository.GetAll();
        decimal balance = 0;

        foreach (var t in transactions)
        {
            // Deposits to this account increase balance
            if (t.Type == TransactionType.Deposit && t.ToAccount == accountId)
                balance += t.Amount;

            // Withdrawals from this account decrease balance
            if (t.Type == TransactionType.Withdrawal && t.FromAccount == accountId)
                balance -= t.Amount;

            // Transfers: decrease sender, increase receiver
            if (t.Type == TransactionType.Transfer)
            {
                if (t.FromAccount == accountId)
                    balance -= t.Amount;
                if (t.ToAccount == accountId)
                    balance += t.Amount;
            }
        }

        return balance;
    }

    public AccountSummaryResponse GetSummary(string accountId)
    {
        var accountTransactions = _repository.GetAll()
            .Where(t => t.FromAccount == accountId || t.ToAccount == accountId)
            .ToList();

        decimal totalDeposits = 0;
        decimal totalWithdrawals = 0;

        foreach (var t in accountTransactions)
        {
            if (t.Type == TransactionType.Deposit && t.ToAccount == accountId)
                totalDeposits += t.Amount;

            if (t.Type == TransactionType.Withdrawal && t.FromAccount == accountId)
                totalWithdrawals += t.Amount;

            if (t.Type == TransactionType.Transfer)
            {
                if (t.ToAccount == accountId)
                    totalDeposits += t.Amount;
                if (t.FromAccount == accountId)
                    totalWithdrawals += t.Amount;
            }
        }

        var mostRecent = accountTransactions
            .OrderByDescending(t => t.Timestamp)
            .FirstOrDefault();

        return new AccountSummaryResponse
        {
            AccountId = accountId,
            TotalDeposits = totalDeposits,
            TotalWithdrawals = totalWithdrawals,
            TransactionCount = accountTransactions.Count,
            MostRecentTransactionDate = mostRecent?.Timestamp.ToString("o")
        };
    }
}
