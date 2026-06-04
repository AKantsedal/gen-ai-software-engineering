namespace BankingApi.Tests;

public class TransactionValidatorTests
{
    [Fact]
    public void ValidateAmount_WithPositiveAmount_HasNoErrors()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var request = new CreateTransactionRequest
        {
            Type = "deposit",
            Amount = 100m,
            Currency = "USD",
            ToAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        var amountErrors = errors.Where(e => e.Field == "amount").ToList();
        Assert.Empty(amountErrors);
    }

    [Fact]
    public void ValidateAmount_WithZeroAmount_HasError()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Fix 2: zero amount should be rejected (if (amount <= 0), not < 0)
        var request = new CreateTransactionRequest
        {
            Type = "deposit",
            Amount = 0m,
            Currency = "USD",
            ToAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        var amountError = errors.FirstOrDefault(e => e.Field == "amount");
        Assert.NotNull(amountError);
        Assert.Equal("Amount must be a positive number.", amountError.Message);
    }

    [Fact]
    public void ValidateAmount_WithNegativeAmount_HasError()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var request = new CreateTransactionRequest
        {
            Type = "deposit",
            Amount = -50m,
            Currency = "USD",
            ToAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        var amountError = errors.FirstOrDefault(e => e.Field == "amount");
        Assert.NotNull(amountError);
        Assert.Equal("Amount must be a positive number.", amountError.Message);
    }

    [Fact]
    public void ValidateAmount_WithValidDecimal_HasNoErrors()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Edge case: valid 2 decimal places
        var request = new CreateTransactionRequest
        {
            Type = "deposit",
            Amount = 123.45m,
            Currency = "USD",
            ToAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        var amountErrors = errors.Where(e => e.Field == "amount").ToList();
        Assert.Empty(amountErrors);
    }

    [Fact]
    public void ValidateAmount_WithTooManyDecimals_HasError()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Edge case: more than 2 decimal places
        var request = new CreateTransactionRequest
        {
            Type = "deposit",
            Amount = 123.456m,
            Currency = "USD",
            ToAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        var amountError = errors.FirstOrDefault(e => e.Field == "amount");
        Assert.NotNull(amountError);
        Assert.Equal("Amount must have at most 2 decimal places.", amountError.Message);
    }

    [Fact]
    public void ValidateAmount_WithSmallPositiveAmount_HasNoErrors()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Edge case: minimum positive amount
        var request = new CreateTransactionRequest
        {
            Type = "deposit",
            Amount = 0.01m,
            Currency = "USD",
            ToAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        var amountErrors = errors.Where(e => e.Field == "amount").ToList();
        Assert.Empty(amountErrors);
    }

    [Fact]
    public void Validate_WithValidDeposit_HasNoErrors()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Happy path: valid deposit request
        var request = new CreateTransactionRequest
        {
            Type = "deposit",
            Amount = 100m,
            Currency = "USD",
            ToAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithValidWithdrawal_HasNoErrors()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var request = new CreateTransactionRequest
        {
            Type = "withdrawal",
            Amount = 50m,
            Currency = "USD",
            FromAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithValidTransfer_HasNoErrors()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var request = new CreateTransactionRequest
        {
            Type = "transfer",
            Amount = 75m,
            Currency = "USD",
            FromAccount = "ACC-12345",
            ToAccount = "ACC-54321"
        };

        var errors = TransactionValidator.Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithZeroAmountInWithdrawal_HasError()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        // Fix 2 validation: zero amount should fail
        var request = new CreateTransactionRequest
        {
            Type = "withdrawal",
            Amount = 0m,
            Currency = "USD",
            FromAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        var amountError = errors.FirstOrDefault(e => e.Field == "amount");
        Assert.NotNull(amountError);
    }

    [Fact]
    public void Validate_WithInvalidType_HasError()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var request = new CreateTransactionRequest
        {
            Type = "invalid",
            Amount = 100m,
            Currency = "USD",
            ToAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        var typeError = errors.FirstOrDefault(e => e.Field == "type");
        Assert.NotNull(typeError);
    }

    [Fact]
    public void Validate_WithInvalidCurrency_HasError()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var request = new CreateTransactionRequest
        {
            Type = "deposit",
            Amount = 100m,
            Currency = "INVALID",
            ToAccount = "ACC-12345"
        };

        var errors = TransactionValidator.Validate(request);

        var currencyError = errors.FirstOrDefault(e => e.Field == "currency");
        Assert.NotNull(currencyError);
    }

    [Fact]
    public void IsValidAccountId_WithValidFormat_ReturnsTrue()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var result = TransactionValidator.IsValidAccountId("ACC-12345");

        Assert.True(result);
    }

    [Fact]
    public void IsValidAccountId_WithInvalidFormat_ReturnsFalse()
    {
        // FIRST: F✓ I✓ R✓ S✓ T✓
        var result = TransactionValidator.IsValidAccountId("INVALID");

        Assert.False(result);
    }
}
