namespace BankingApi.Tests;

public class AccountsControllerTests
{
    private class MockTransactionService : ITransactionService
    {
        private readonly Dictionary<string, decimal> _balances = [];
        private readonly Dictionary<string, AccountSummaryResponse> _summaries = [];

        public void SetBalance(string accountId, decimal balance) => _balances[accountId] = balance;
        public void SetSummary(string accountId, AccountSummaryResponse summary) => _summaries[accountId] = summary;

        public Transaction Create(string fromAccount, string toAccount, decimal amount, Currency currency, TransactionType type)
            => throw new NotImplementedException();

        public IEnumerable<Transaction> GetAll()
            => throw new NotImplementedException();

        public Transaction? GetById(string id)
            => throw new NotImplementedException();

        public decimal GetBalance(string accountId)
            => _balances.TryGetValue(accountId, out var balance) ? balance : 0m;

        public AccountSummaryResponse GetSummary(string accountId)
            => _summaries.TryGetValue(accountId, out var summary)
                ? summary
                : new AccountSummaryResponse { AccountId = accountId, TotalDeposits = 0, TotalWithdrawals = 0 };
    }

    [Fact]
    public void GetBalance_WithValidAccount_ReturnsOk()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Fix 3 validation: normal endpoints still work after debug endpoint removal
        var mockService = new MockTransactionService();
        mockService.SetBalance("ACC-12345", 1000m);
        var controller = new AccountsController(mockService);

        var result = controller.GetBalance("ACC-12345");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AccountBalanceResponse>(okResult.Value);
        Assert.Equal("ACC-12345", response.AccountId);
        Assert.Equal(1000m, response.Balance);
    }

    [Fact]
    public void GetBalance_WithInvalidAccountFormat_ReturnsBadRequest()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var mockService = new MockTransactionService();
        var controller = new AccountsController(mockService);

        var result = controller.GetBalance("INVALID");

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ValidationErrorResponse>(badRequestResult.Value);
        Assert.NotEmpty(errorResponse.Details);
    }

    [Fact]
    public void GetBalance_WithAccountIdTrimming_StripsWhitespace()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Edge case: account ID with surrounding whitespace
        var mockService = new MockTransactionService();
        mockService.SetBalance("ACC-12345", 500m);
        var controller = new AccountsController(mockService);

        var result = controller.GetBalance("  ACC-12345  ");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AccountBalanceResponse>(okResult.Value);
        Assert.Equal(500m, response.Balance);
    }

    [Fact]
    public void GetBalance_WithZeroBalance_ReturnsZero()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var mockService = new MockTransactionService();
        mockService.SetBalance("ACC-12345", 0m);
        var controller = new AccountsController(mockService);

        var result = controller.GetBalance("ACC-12345");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AccountBalanceResponse>(okResult.Value);
        Assert.Equal(0m, response.Balance);
    }

    [Fact]
    public void GetSummary_WithValidAccount_ReturnsOk()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var mockService = new MockTransactionService();
        var summary = new AccountSummaryResponse
        {
            AccountId = "ACC-12345",
            TotalDeposits = 1000m,
            TotalWithdrawals = 300m,
            TransactionCount = 5
        };
        mockService.SetSummary("ACC-12345", summary);
        var controller = new AccountsController(mockService);

        var result = controller.GetSummary("ACC-12345");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AccountSummaryResponse>(okResult.Value);
        Assert.Equal("ACC-12345", response.AccountId);
        Assert.Equal(1000m, response.TotalDeposits);
        Assert.Equal(300m, response.TotalWithdrawals);
    }

    [Fact]
    public void GetSummary_WithInvalidAccountFormat_ReturnsBadRequest()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var mockService = new MockTransactionService();
        var controller = new AccountsController(mockService);

        var result = controller.GetSummary("invalid-format");

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ValidationErrorResponse>(badRequestResult.Value);
        Assert.NotEmpty(errorResponse.Details);
    }

    [Fact]
    public void GetSummary_WithAccountIdTrimming_StripsWhitespace()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var mockService = new MockTransactionService();
        var summary = new AccountSummaryResponse
        {
            AccountId = "ACC-54321",
            TotalDeposits = 500m,
            TotalWithdrawals = 100m
        };
        mockService.SetSummary("ACC-54321", summary);
        var controller = new AccountsController(mockService);

        var result = controller.GetSummary("  ACC-54321  ");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AccountSummaryResponse>(okResult.Value);
        Assert.Equal(500m, response.TotalDeposits);
    }

    [Fact]
    public void GetBalance_BalanceResponseIncludesCurrency()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var mockService = new MockTransactionService();
        mockService.SetBalance("ACC-12345", 100m);
        var controller = new AccountsController(mockService);

        var result = controller.GetBalance("ACC-12345");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AccountBalanceResponse>(okResult.Value);
        Assert.Equal(Currency.USD, response.Currency);
    }
}
