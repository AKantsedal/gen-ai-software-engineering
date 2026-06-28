using System.Text.Json;
using BankingPipeline.Models;
using BankingPipeline.Tests.Helpers;
using Xunit;

namespace BankingPipeline.Tests;

public class IntegrationTests : IDisposable
{
    private readonly string _root;
    private readonly string _originalDir;
    private static readonly string SampleTransactionsPath =
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "sample-transactions.json"));

    public IntegrationTests()
    {
        _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(_root, "shared", "input"));
        Directory.CreateDirectory(Path.Combine(_root, "shared", "processing"));
        Directory.CreateDirectory(Path.Combine(_root, "shared", "output"));
        Directory.CreateDirectory(Path.Combine(_root, "shared", "results"));
        _originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_root);
    }

    [Fact]
    public async Task FullPipeline_AllEightTransactions_ProduceResultFiles()
    {
        // Copy sample-transactions.json to temp root
        File.Copy(SampleTransactionsPath, Path.Combine(_root, "sample-transactions.json"));

        var integrator = new Integrator(TestFixture.LoggerFactory, TestFixture.JsonOptions);
        await integrator.RunAsync("sample-transactions.json");

        var resultFiles = Directory.GetFiles(Path.Combine("shared", "results"), "TXN*.json");
        Assert.Equal(8, resultFiles.Length);
    }

    [Fact]
    public async Task FullPipeline_SummaryJsonExists_WithCorrectTotals()
    {
        File.Copy(SampleTransactionsPath, Path.Combine(_root, "sample-transactions.json"));

        var integrator = new Integrator(TestFixture.LoggerFactory, TestFixture.JsonOptions);
        await integrator.RunAsync("sample-transactions.json");

        var summaryPath = Path.Combine("shared", "results", "summary.json");
        Assert.True(File.Exists(summaryPath));

        var summary = JsonSerializer.Deserialize<PipelineSummary>(
            await File.ReadAllTextAsync(summaryPath), TestFixture.JsonOptions);

        Assert.NotNull(summary);
        Assert.Equal(8, summary.Total);
        Assert.Equal(1, summary.Rejected);
        Assert.Equal(2, summary.Flagged);
        Assert.Equal(5, summary.Approved);
        Assert.Equal(5, summary.Settled);
        Assert.Equal(2, summary.HeldForReview);
    }

    [Fact]
    public async Task FullPipeline_SharedDirsEmpty_AfterRun()
    {
        File.Copy(SampleTransactionsPath, Path.Combine(_root, "sample-transactions.json"));

        var integrator = new Integrator(TestFixture.LoggerFactory, TestFixture.JsonOptions);
        await integrator.RunAsync("sample-transactions.json");

        Assert.Empty(Directory.GetFiles(Path.Combine("shared", "input")));
        Assert.Empty(Directory.GetFiles(Path.Combine("shared", "processing")));
        Assert.Empty(Directory.GetFiles(Path.Combine("shared", "output")));
    }

    [Fact]
    public async Task FullPipeline_TXN006_RejectedWithInvalidCurrency()
    {
        File.Copy(SampleTransactionsPath, Path.Combine(_root, "sample-transactions.json"));

        var integrator = new Integrator(TestFixture.LoggerFactory, TestFixture.JsonOptions);
        await integrator.RunAsync("sample-transactions.json");

        var txn006Path = Path.Combine("shared", "results", "TXN006.json");
        Assert.True(File.Exists(txn006Path));

        var envelope = JsonSerializer.Deserialize<MessageEnvelope>(
            await File.ReadAllTextAsync(txn006Path), TestFixture.JsonOptions);

        Assert.NotNull(envelope);
        Assert.Equal("rejected", envelope.Data.Status);
        Assert.Equal("invalid_currency_code", envelope.Data.RejectionReason);
    }

    [Fact]
    public async Task FullPipeline_TXN002AndTXN005_HeldForReview()
    {
        File.Copy(SampleTransactionsPath, Path.Combine(_root, "sample-transactions.json"));

        var integrator = new Integrator(TestFixture.LoggerFactory, TestFixture.JsonOptions);
        await integrator.RunAsync("sample-transactions.json");

        foreach (var txnId in new[] { "TXN002", "TXN005" })
        {
            var envelope = JsonSerializer.Deserialize<MessageEnvelope>(
                await File.ReadAllTextAsync(Path.Combine("shared", "results", $"{txnId}.json")),
                TestFixture.JsonOptions);

            Assert.NotNull(envelope);
            Assert.Equal("held_for_review", envelope!.Data.SettlementStatus);
        }
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
        Directory.Delete(_root, recursive: true);
    }
}
