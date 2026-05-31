namespace BankingApi.Tests;

public class TransactionServiceTests
{
    private class MockTransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions = [];

        public void Add(Transaction transaction) => _transactions.Add(transaction);
        public IEnumerable<Transaction> GetAll() => _transactions;
        public Transaction? GetById(string id) => _transactions.FirstOrDefault(t => t.Id == id);
    }

    [Fact]
    public void GetBalance_WithDeposit_IncreasesBalance()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // No I/O, fresh repo per test, no time/random deps, clear assertion, tests the fixed code
        var repo = new MockTransactionRepository();
        var service = new TransactionService(repo);

        var deposit = new Transaction
        {
            Id = "1",
            ToAccount = "ACC-12345",
            Amount = 100m,
            Currency = Currency.USD,
            Type = TransactionType.Deposit,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        };
        repo.Add(deposit);

        var balance = service.GetBalance("ACC-12345");

        Assert.Equal(100m, balance);
    }

    [Fact]
    public void GetBalance_WithWithdrawal_DecreasesBalance()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Fix 1: withdrawal should use -= not += (this test validates the fix)
        var repo = new MockTransactionRepository();
        var service = new TransactionService(repo);

        var deposit = new Transaction
        {
            Id = "1",
            ToAccount = "ACC-12345",
            Amount = 100m,
            Currency = Currency.USD,
            Type = TransactionType.Deposit,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        };
        var withdrawal = new Transaction
        {
            Id = "2",
            FromAccount = "ACC-12345",
            Amount = 30m,
            Currency = Currency.USD,
            Type = TransactionType.Withdrawal,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        };
        repo.Add(deposit);
        repo.Add(withdrawal);

        var balance = service.GetBalance("ACC-12345");

        Assert.Equal(70m, balance);
    }

    [Fact]
    public void GetBalance_WithTransferOut_DecreasesBalance()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var repo = new MockTransactionRepository();
        var service = new TransactionService(repo);

        var transfer = new Transaction
        {
            Id = "1",
            FromAccount = "ACC-12345",
            ToAccount = "ACC-54321",
            Amount = 50m,
            Currency = Currency.USD,
            Type = TransactionType.Transfer,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        };
        repo.Add(transfer);

        var balance = service.GetBalance("ACC-12345");

        Assert.Equal(-50m, balance);
    }

    [Fact]
    public void GetBalance_WithTransferIn_IncreasesBalance()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var repo = new MockTransactionRepository();
        var service = new TransactionService(repo);

        var transfer = new Transaction
        {
            Id = "1",
            FromAccount = "ACC-54321",
            ToAccount = "ACC-12345",
            Amount = 50m,
            Currency = Currency.USD,
            Type = TransactionType.Transfer,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        };
        repo.Add(transfer);

        var balance = service.GetBalance("ACC-12345");

        Assert.Equal(50m, balance);
    }

    [Fact]
    public void GetBalance_WithNoTransactions_ReturnsZero()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var repo = new MockTransactionRepository();
        var service = new TransactionService(repo);

        var balance = service.GetBalance("ACC-12345");

        Assert.Equal(0m, balance);
    }

    [Fact]
    public void GetBalance_WithMixedTransactions_CalculatesCorrectly()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Edge case: multiple different transaction types
        var repo = new MockTransactionRepository();
        var service = new TransactionService(repo);

        repo.Add(new Transaction
        {
            Id = "1",
            ToAccount = "ACC-12345",
            Amount = 1000m,
            Currency = Currency.USD,
            Type = TransactionType.Deposit,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        });
        repo.Add(new Transaction
        {
            Id = "2",
            FromAccount = "ACC-12345",
            Amount = 200m,
            Currency = Currency.USD,
            Type = TransactionType.Withdrawal,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        });
        repo.Add(new Transaction
        {
            Id = "3",
            FromAccount = "ACC-12345",
            ToAccount = "ACC-54321",
            Amount = 300m,
            Currency = Currency.USD,
            Type = TransactionType.Transfer,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        });
        repo.Add(new Transaction
        {
            Id = "4",
            FromAccount = "ACC-54321",
            ToAccount = "ACC-12345",
            Amount = 100m,
            Currency = Currency.USD,
            Type = TransactionType.Transfer,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        });

        var balance = service.GetBalance("ACC-12345");

        // 1000 - 200 - 300 + 100 = 600
        Assert.Equal(600m, balance);
    }

    [Fact]
    public void GetBalance_IgnoresTransactionsForOtherAccounts()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var repo = new MockTransactionRepository();
        var service = new TransactionService(repo);

        repo.Add(new Transaction
        {
            Id = "1",
            ToAccount = "ACC-AAAAA",
            Amount = 100m,
            Currency = Currency.USD,
            Type = TransactionType.Deposit,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        });

        var balance = service.GetBalance("ACC-12345");

        Assert.Equal(0m, balance);
    }

    [Fact]
    public void GetBalance_WithDecimalAmounts_CalculatesCorrectly()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Edge case: precise decimal values
        var repo = new MockTransactionRepository();
        var service = new TransactionService(repo);

        repo.Add(new Transaction
        {
            Id = "1",
            ToAccount = "ACC-12345",
            Amount = 123.45m,
            Currency = Currency.USD,
            Type = TransactionType.Deposit,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        });
        repo.Add(new Transaction
        {
            Id = "2",
            FromAccount = "ACC-12345",
            Amount = 45.67m,
            Currency = Currency.USD,
            Type = TransactionType.Withdrawal,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed
        });

        var balance = service.GetBalance("ACC-12345");

        Assert.Equal(77.78m, balance);
    }
}
