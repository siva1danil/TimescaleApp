namespace Services.Interfaces.Models;

public sealed record ResultModel(
    string Filename,
    double DateDeltaSeconds,
    DateTimeOffset FirstOperationDate,
    double AverageExecutionTime,
    double AverageValue,
    double MedianValue,
    double MaxValue,
    double MinValue);
