using System.Collections.Concurrent;
using BankingApi.Models;

namespace BankingApi.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ConcurrentDictionary<string, Transaction> _transactions = new();

    public void Add(Transaction transaction)
    {
        _transactions[transaction.Id] = transaction;
    }

    public IEnumerable<Transaction> GetAll()
    {
        return _transactions.Values;
    }

    public Transaction? GetById(string id)
    {
        _transactions.TryGetValue(id, out var transaction);
        return transaction;
    }
}
