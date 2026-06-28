using BankingPipeline.Agents;
using BankingPipeline.Helpers;
using BankingPipeline.Tests.Helpers;
using Xunit;

namespace BankingPipeline.Tests;

public class FraudDetectorTests : IDisposable
{
    private readonly string _root;
    private readonly string _output;
    private readonly string _processing;
    private readonly FraudDetector _detector;
    private readonly string _originalDir;

    public FraudDetectorTests()
    {
        (_root, _, _processing, _output, _) = TestFixture.CreateTempDirs();
        _originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_root);
        _detector = new FraudDetector(TestFixture.LoggerFactory, TestFixture.JsonOptions);
    }

    private async Task<string> WriteOutputFile(
        string txnId, string amount, string currency = "USD",
        string country = "US", string timestamp = "2026-03-16T09:00:00Z")
    {
        var envelope = EnvelopeFactory.CreateValidated(txnId, amount, currency, "transfer", country, timestamp);
        var path = Path.Combine("shared", "output", $"{txnId}.json");
        await FileHelper.WriteJsonAtomicAsync(path, envelope, TestFixture.JsonOptions);
        return path;
    }

    [Fact]
    public async Task NormalTransaction_RiskZero_ReturnsApproved()
    {
        var path = await WriteOutputFile("TXN001", "1500.00");
        var status = await _detector.ProcessAsync(path);

        Assert.Equal("approved", status);
        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "processing", "TXN001.json"), TestFixture.JsonOptions);
        Assert.Equal(0, result.Data.RiskScore);
        Assert.Equal("approved", result.Data.Status);
    }

    [Fact]
    public async Task HighValue_25k_Risk60_ReturnsFlagged()
    {
        var path = await WriteOutputFile("TXN002", "25000.00");
        var status = await _detector.ProcessAsync(path);

        Assert.Equal("flagged", status);
        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "processing", "TXN002.json"), TestFixture.JsonOptions);
        Assert.Equal(60, result.Data.RiskScore);
    }

    [Fact]
    public async Task Structuring_9999_Risk40_ReturnsApproved()
    {
        // 40 < 50, so approved
        var path = await WriteOutputFile("TXN003", "9999.99");
        var status = await _detector.ProcessAsync(path);

        Assert.Equal("approved", status);
        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "processing", "TXN003.json"), TestFixture.JsonOptions);
        Assert.Equal(40, result.Data.RiskScore);
    }

    [Fact]
    public async Task OffHours_Adds20ToScore()
    {
        // Off-hours (02:47 UTC) + cross-border (DE) = 0 + 20 + 15 = 35
        var path = await WriteOutputFile("TXN004", "500.00", "EUR", "DE", "2026-03-16T02:47:00Z");
        var status = await _detector.ProcessAsync(path);

        Assert.Equal("approved", status);
        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "processing", "TXN004.json"), TestFixture.JsonOptions);
        Assert.Equal(35, result.Data.RiskScore);
    }

    [Fact]
    public async Task HighValue_75k_Risk60_ReturnsFlagged()
    {
        var path = await WriteOutputFile("TXN005", "75000.00");
        var status = await _detector.ProcessAsync(path);

        Assert.Equal("flagged", status);
        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "processing", "TXN005.json"), TestFixture.JsonOptions);
        Assert.Equal(60, result.Data.RiskScore);
    }

    [Fact]
    public async Task CrossBorder_Adds15ToScore()
    {
        // -100 GBP refund, GB country = cross-border 15
        var envelope = EnvelopeFactory.CreateValidated("TXN007", "-100.00", "GBP", "refund", "GB");
        var path = Path.Combine("shared", "output", "TXN007.json");
        await FileHelper.WriteJsonAtomicAsync(path, envelope, TestFixture.JsonOptions);

        await _detector.ProcessAsync(path);

        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "processing", "TXN007.json"), TestFixture.JsonOptions);
        Assert.Equal(15, result.Data.RiskScore);
        Assert.Equal("approved", result.Data.Status);
    }

    [Fact]
    public async Task HighValuePlusOffHours_ScoreIs80_ReturnsFlagged()
    {
        // 60 (high-value) + 20 (off-hours) = 80
        var path = await WriteOutputFile("TXN_HV_OH", "25000.00", "USD", "US", "2026-03-16T03:00:00Z");
        var status = await _detector.ProcessAsync(path);

        Assert.Equal("flagged", status);
        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "processing", "TXN_HV_OH.json"), TestFixture.JsonOptions);
        Assert.Equal(80, result.Data.RiskScore);
    }

    [Fact]
    public async Task AllSignals_ScoreCappedAt100()
    {
        // 60 (high-value) + 20 (off-hours) + 15 (cross-border) = 95, capped at 100
        var path = await WriteOutputFile("TXN_CAP", "25000.00", "EUR", "DE", "2026-03-16T03:00:00Z");
        await _detector.ProcessAsync(path);

        var result = await FileHelper.ReadJsonAsync<Models.MessageEnvelope>(
            Path.Combine("shared", "processing", "TXN_CAP.json"), TestFixture.JsonOptions);
        Assert.True(result.Data.RiskScore <= 100);
        Assert.Equal(95, result.Data.RiskScore);
    }

    [Fact]
    public async Task SourceFileDeletedAfterProcessing()
    {
        var path = await WriteOutputFile("TXN_DEL2", "1000.00");
        await _detector.ProcessAsync(path);
        Assert.False(File.Exists(path));
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
        Directory.Delete(_root, recursive: true);
    }
}
