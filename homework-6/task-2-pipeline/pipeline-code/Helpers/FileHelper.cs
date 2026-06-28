using System.Text.Json;

namespace BankingPipeline.Helpers;

public static class FileHelper
{
    public static async Task WriteJsonAtomicAsync<T>(
        string filePath, T data, JsonSerializerOptions options, CancellationToken ct = default)
    {
        var tmpPath = filePath + ".tmp";
        await using (var stream = File.Create(tmpPath))
        {
            await JsonSerializer.SerializeAsync(stream, data, options, ct);
        }
        File.Move(tmpPath, filePath, overwrite: true);
    }

    public static async Task<T> ReadJsonAsync<T>(
        string filePath, JsonSerializerOptions options, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<T>(stream, options, ct)
               ?? throw new InvalidOperationException($"Failed to deserialize {filePath}");
    }
}
