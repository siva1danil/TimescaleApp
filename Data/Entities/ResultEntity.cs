namespace Data.Entities;

public sealed class ResultEntity
{
    public long Id { get; set; }

    public required string Filename { get; set; }

    public double DateDeltaSeconds { get; set; }

    public DateTimeOffset FirstOperationDate { get; set; }

    public double AverageExecutionTime { get; set; }

    public double AverageValue { get; set; }

    public double MedianValue { get; set; }

    public double MaxValue { get; set; }

    public double MinValue { get; set; }

    public ICollection<ValueEntity> Values { get; set; } = [];
}
