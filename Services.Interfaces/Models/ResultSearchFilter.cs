namespace Services.Interfaces.Models;

public sealed record ResultSearchFilter
{
    public string? Filename { get; init; }

    public DateTimeOffset? FirstOperationDateFrom { get; init; }

    public DateTimeOffset? FirstOperationDateTo { get; init; }

    public double? AverageValueFrom { get; init; }

    public double? AverageValueTo { get; init; }

    public double? AverageExecutionTimeFrom { get; init; }

    public double? AverageExecutionTimeTo { get; init; }
}
