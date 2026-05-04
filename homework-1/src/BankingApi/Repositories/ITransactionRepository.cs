using BankingApi.Models;

namespace BankingApi.Repositories;

public interface ITransactionRepository
{
    void Add(Transaction transaction);
    IEnumerable<Transaction> GetAll();
    Transaction? GetById(string id);
}
