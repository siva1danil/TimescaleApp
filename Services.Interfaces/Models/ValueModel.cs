namespace Services.Interfaces.Models;

public sealed record ValueModel(
    DateTimeOffset Date,
    double ExecutionTime,
    double Value);
