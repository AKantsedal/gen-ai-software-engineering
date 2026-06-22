using BankingPipeline.Helpers;
using BankingPipeline.Models;
using Xunit;

namespace BankingPipeline.Tests.Helpers;

public class FileHelperTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public FileHelperTests() => Directory.CreateDirectory(_tempDir);

    [Fact]
    public async Task WriteJsonAtomicAsync_CreatesFileWithValidContent()
    {
        var filePath = Path.Combine(_tempDir, "test.json");
        var envelope = EnvelopeFactory.CreatePending();

        await FileHelper.WriteJsonAtomicAsync(filePath, envelope, TestFixture.JsonOptions);

        Assert.True(File.Exists(filePath));
        var content = await File.ReadAllTextAsync(filePath);
        Assert.Contains("transaction_id", content);
    }

    [Fact]
    public async Task WriteJsonAtomicAsync_DoesNotLeaveTmpFile()
    {
        var filePath = Path.Combine(_tempDir, "test.json");
        var envelope = EnvelopeFactory.CreatePending();

        await FileHelper.WriteJsonAtomicAsync(filePath, envelope, TestFixture.JsonOptions);

        Assert.False(File.Exists(filePath + ".tmp"));
    }

    [Fact]
    public async Task WriteJsonAtomicAsync_OverwritesExistingFile()
    {
        var filePath = Path.Combine(_tempDir, "test.json");
        await FileHelper.WriteJsonAtomicAsync(filePath, EnvelopeFactory.CreatePending("TXN001"), TestFixture.JsonOptions);
        await FileHelper.WriteJsonAtomicAsync(filePath, EnvelopeFactory.CreatePending("TXN002"), TestFixture.JsonOptions);

        var result = await FileHelper.ReadJsonAsync<MessageEnvelope>(filePath, TestFixture.JsonOptions);
        Assert.Equal("TXN002", result.Data.TransactionId);
    }

    [Fact]
    public async Task ReadJsonAsync_DeserializesCorrectly()
    {
        var filePath = Path.Combine(_tempDir, "test.json");
        var original = EnvelopeFactory.CreatePending("TXN099", "5000.00", "EUR");
        await FileHelper.WriteJsonAtomicAsync(filePath, original, TestFixture.JsonOptions);

        var result = await FileHelper.ReadJsonAsync<MessageEnvelope>(filePath, TestFixture.JsonOptions);

        Assert.Equal("TXN099", result.Data.TransactionId);
        Assert.Equal("5000.00", result.Data.Amount);
        Assert.Equal("EUR", result.Data.Currency);
    }

    [Fact]
    public async Task ReadJsonAsync_ThrowsOnMissingFile()
    {
        await Assert.ThrowsAnyAsync<Exception>(() =>
            FileHelper.ReadJsonAsync<MessageEnvelope>(
                Path.Combine(_tempDir, "nonexistent.json"), TestFixture.JsonOptions));
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);
}
