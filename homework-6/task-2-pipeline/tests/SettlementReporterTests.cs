using BankingPipeline.Agents;
using BankingPipeline.Helpers;
using BankingPipeline.Tests.Helpers;
using Xunit;

namespace BankingPipeline.Tests;

public class SettlementReporterTests : IDisposable
{
    private readonly string _root;
    private readonly string _processing;
    private readonly string _results;
    private readonly SettlementReporter _reporter;
    private readonly string _originalDir;

    public SettlementReporterTests()
    {
        (_root, _, _processing, _, _results) = TestFixture.CreateTempDirs();
        _originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_root);
        _reporter = new SettlementReporter(TestFixture.LoggerFactory, TestFixture.JsonOptions);
    }

    private async Task<string> WriteProcessingFile(string txnId, string status, int riskScore = 0)
    {
        var envelope = EnvelopeFactory.CreateValidated(txnId, "1500.00", "USD", "transfer", "US");
        envelope.Data.Status = status;
        envelope.Data.RiskScore = riskScore;
        envelope.SourceAgent = "fraud_detector";
        envelope.TargetAgent = "settlement_reporter";
        var path = Path.Combine("shared", "processing", $"{txnId}.json");
        await FileHelper.WriteJsonAtomicAsync(path, envelope, TestFixture.JsonOptions);
        return path;
    }

    [Fact]
    public async Task ApprovedTransaction_ReturnsSettled()
    {
        var path = await WriteProcessingFile("TXN001", "approved", 0);
        var result = await _reporter.ProcessAsync(path);

        Assert.Equal("settled", result);
        var resultFile = Path.Combine("shared", "results", "TXN001.json");
        Assert.True(File.Exists(resultFile));
        var envelope = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(resultFile, TestFixture.JsonOptions);
        Assert.Equal("settled", envelope.Data.SettlementStatus);
    }

    [Fact]
    public async Task FlaggedTransaction_ReturnsHeldForReview()
    {
        var path = await WriteProcessingFile("TXN002", "flagged", 60);
        var result = await _reporter.ProcessAsync(path);

        Assert.Equal("held_for_review", result);
        var resultFile = Path.Combine("shared", "results", "TXN002.json");
        var envelope = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(resultFile, TestFixture.JsonOptions);
        Assert.Equal("held_for_review", envelope.Data.SettlementStatus);
    }

    [Fact]
    public async Task ResultFileWrittenToResultsDir()
    {
        var path = await WriteProcessingFile("TXN008", "approved");
        await _reporter.ProcessAsync(path);
        Assert.True(File.Exists(Path.Combine("shared", "results", "TXN008.json")));
    }

    [Fact]
    public async Task SourceFileDeletedAfterProcessing()
    {
        var path = await WriteProcessingFile("TXN_DEL", "approved");
        await _reporter.ProcessAsync(path);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task UnexpectedStatus_ThrowsInvalidOperationException()
    {
        var path = await WriteProcessingFile("TXN_BAD", "unknown_status");
        await Assert.ThrowsAsync<InvalidOperationException>(() => _reporter.ProcessAsync(path));
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
        Directory.Delete(_root, recursive: true);
    }
}
