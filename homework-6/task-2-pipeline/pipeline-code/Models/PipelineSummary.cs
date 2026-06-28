using System.Text.Json.Serialization;

namespace BankingPipeline.Models;

public class PipelineSummary
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("validated")]
    public int Validated { get; set; }

    [JsonPropertyName("rejected")]
    public int Rejected { get; set; }

    [JsonPropertyName("flagged")]
    public int Flagged { get; set; }

    [JsonPropertyName("approved")]
    public int Approved { get; set; }

    [JsonPropertyName("settled")]
    public int Settled { get; set; }

    [JsonPropertyName("held_for_review")]
    public int HeldForReview { get; set; }

    [JsonPropertyName("pipeline_run_timestamp")]
    public string PipelineRunTimestamp { get; set; } = string.Empty;
}
