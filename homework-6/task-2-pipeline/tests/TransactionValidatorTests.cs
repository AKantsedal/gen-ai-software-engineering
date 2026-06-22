using BankingPipeline.Agents;
using BankingPipeline.Helpers;
using BankingPipeline.Tests.Helpers;
using Xunit;

namespace BankingPipeline.Tests;

public class TransactionValidatorTests : IDisposable
{
    private readonly string _root;
    private readonly string _processing;
    private readonly string _output;
    private readonly string _results;
    private readonly TransactionValidator _validator;
    private readonly string _originalDir;

    public TransactionValidatorTests()
    {
        (_root, _, _processing, _output, _results) = TestFixture.CreateTempDirs();
        _originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_root);
        _validator = new TransactionValidator(TestFixture.LoggerFactory, TestFixture.JsonOptions);
    }

    private async Task<string> WriteProcessingFile(
        string txnId = "TXN001", string amount = "1500.00",
        string currency = "USD", string type = "transfer",
        string country = "US", string timestamp = "2026-03-16T09:00:00Z")
    {
        var envelope = EnvelopeFactory.CreatePending(txnId, amount, currency, type, country, timestamp);
        var path = Path.Combine("shared", "processing", $"{txnId}.json");
        await FileHelper.WriteJsonAtomicAsync(path, envelope, TestFixture.JsonOptions);
        return path;
    }

    [Fact]
    public async Task ValidTransaction_WritesToOutput_ReturnsValidated()
    {
        var path = await WriteProcessingFile();
        var status = await _validator.ProcessAsync(path);

        Assert.Equal("validated", status);
        Assert.True(File.Exists(Path.Combine("shared", "output", "TXN001.json")));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task InvalidCurrency_WritesToResults_ReturnsRejected()
    {
        var path = await WriteProcessingFile("TXN006", "200.00", "XYZ");
        var status = await _validator.ProcessAsync(path);

        Assert.Equal("rejected", status);
        var resultFile = Path.Combine("shared", "results", "TXN006.json");
        Assert.True(File.Exists(resultFile));
        var envelope = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(resultFile, TestFixture.JsonOptions);
        Assert.Equal("rejected", envelope.Data.Status);
        Assert.Equal("invalid_currency_code", envelope.Data.RejectionReason);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task NegativeAmount_NonRefund_ReturnsRejected()
    {
        var path = await WriteProcessingFile("TXN_NEG", "-100.00", "USD", "transfer");
        var status = await _validator.ProcessAsync(path);

        Assert.Equal("rejected", status);
        var resultFile = Path.Combine("shared", "results", "TXN_NEG.json");
        var envelope = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(resultFile, TestFixture.JsonOptions);
        Assert.Equal("negative_amount", envelope.Data.RejectionReason);
    }

    [Fact]
    public async Task NegativeAmount_Refund_IsValidated()
    {
        // TXN007 - negative refund should pass
        var path = await WriteProcessingFile("TXN007", "-100.00", "GBP", "refund", "GB");
        var status = await _validator.ProcessAsync(path);

        Assert.Equal("validated", status);
        Assert.True(File.Exists(Path.Combine("shared", "output", "TXN007.json")));
    }

    [Fact]
    public async Task MissingTransactionId_ReturnsRejected()
    {
        var envelope = EnvelopeFactory.CreatePending("TXN_MISS");
        envelope.Data.TransactionId = string.Empty;
        var path = Path.Combine("shared", "processing", "TXN_MISS.json");
        await FileHelper.WriteJsonAtomicAsync(path, envelope, TestFixture.JsonOptions);

        var status = await _validator.ProcessAsync(path);

        Assert.Equal("rejected", status);
        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "results", "TXN_MISS.json"), TestFixture.JsonOptions);
        Assert.Equal("missing_required_field", result.Data.RejectionReason);
    }

    [Fact]
    public async Task InvalidAmount_NonParseable_ReturnsRejected()
    {
        var path = await WriteProcessingFile("TXN_BAD", "not-a-number");
        var status = await _validator.ProcessAsync(path);

        Assert.Equal("rejected", status);
        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "results", "TXN_BAD.json"), TestFixture.JsonOptions);
        Assert.Equal("invalid_amount", result.Data.RejectionReason);
    }

    [Theory]
    [InlineData("USD")] [InlineData("EUR")] [InlineData("GBP")] [InlineData("JPY")]
    [InlineData("CAD")] [InlineData("AUD")] [InlineData("CHF")] [InlineData("CNY")]
    [InlineData("SEK")] [InlineData("NOK")] [InlineData("DKK")] [InlineData("SGD")]
    [InlineData("HKD")] [InlineData("NZD")] [InlineData("MXN")] [InlineData("BRL")]
    [InlineData("INR")] [InlineData("ZAR")] [InlineData("KRW")]
    public async Task AllValid_ISO4217_Currencies_AreAccepted(string currency)
    {
        var txnId = $"TXN_{currency}";
        var path = await WriteProcessingFile(txnId, "100.00", currency);
        var status = await _validator.ProcessAsync(path);
        Assert.Equal("validated", status);
    }

    [Fact]
    public async Task SourceFileDeleted_AfterProcessing()
    {
        var path = await WriteProcessingFile("TXN_DEL");
        await _validator.ProcessAsync(path);
        Assert.False(File.Exists(path));
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
        Directory.Delete(_root, recursive: true);
    }
}
