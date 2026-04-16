namespace App1Backend.Entities;

public class AbTestResult
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public int SampleSize { get; set; }
    public decimal MetricValue { get; set; }
    public decimal? PValue { get; set; }
    public decimal? ConfidenceIntervalLower { get; set; }
    public decimal? ConfidenceIntervalUpper { get; set; }
    public bool IsStatisticallySignificant { get; set; }
    public DateTime RecordedAt { get; set; }

    public AbTestVariant Variant { get; set; } = null!;
}
