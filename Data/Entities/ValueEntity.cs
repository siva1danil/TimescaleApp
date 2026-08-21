namespace Data.Entities;

public sealed class ValueEntity
{
    public long Id { get; set; }

    public long ResultId { get; set; }

    public DateTimeOffset Date { get; set; }

    public double ExecutionTime { get; set; }

    public double Value { get; set; }

    public ResultEntity? Result { get; set; }
}
