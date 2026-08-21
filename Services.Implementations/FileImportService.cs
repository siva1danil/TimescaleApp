using System.Globalization;

using Data;
using Data.Entities;

using Microsoft.EntityFrameworkCore;

using Services.Interfaces;

namespace Services.Implementations;

public sealed class FileImportService(
    AppDbContext dbContext,
    TimeProvider timeProvider) : IFileImportService
{
    private const int MaxRows = 10000;
    private const string Header = "Date;ExecutionTime;Value";
    private const string DateFormat = "yyyy-MM-dd'T'HH-mm-ss.ffff'Z'";
    private static readonly DateTimeOffset MinAllowedDate = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeStyles DateStyle = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

    public async Task ImportAsync(
        string filename,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(filename, data);

        var now = timeProvider.GetUtcNow();
        var rows = new List<ValueEntity>(MaxRows);
        var values = new List<double>(MaxRows);

        DateTimeOffset minDate = default;
        DateTimeOffset maxDate = default;
        double averageExecutionTime = 0;
        double averageValue = 0;
        double minValue = 0;
        double maxValue = 0;

        using var reader = new StreamReader(data, leaveOpen: true);

        var header = await reader.ReadLineAsync(cancellationToken);
        if (!string.Equals(header, Header, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"CSV header must be '{Header}'.");
        }

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            var rowNumber = rows.Count + 1;
            var lineNumber = rowNumber + 1;
            if (rowNumber > MaxRows)
            {
                throw new InvalidDataException($"CSV cannot contain more than {MaxRows} lines.");
            }

            var parsed = ParseLine(line, lineNumber, now);
            rows.Add(new ValueEntity
            {
                Date = parsed.Date,
                ExecutionTime = parsed.ExecutionTime,
                Value = parsed.Value
            });
            values.Add(parsed.Value);

            if (rowNumber == 1)
            {
                minDate = parsed.Date;
                maxDate = parsed.Date;
                minValue = parsed.Value;
                maxValue = parsed.Value;
            }
            else
            {
                minDate = parsed.Date < minDate ? parsed.Date : minDate;
                maxDate = parsed.Date > maxDate ? parsed.Date : maxDate;
                minValue = Math.Min(minValue, parsed.Value);
                maxValue = Math.Max(maxValue, parsed.Value);
            }

            averageExecutionTime += (parsed.ExecutionTime - averageExecutionTime) / rowNumber;
            averageValue += (parsed.Value - averageValue) / rowNumber;
        }
        if (rows.Count == 0)
        {
            throw new InvalidDataException("CSV must contain at least one data row.");
        }

        values.Sort();
        var middleIndex = values.Count / 2;
        var medianValue = values.Count % 2 == 1 ? values[middleIndex] : values[middleIndex - 1] + (values[middleIndex] - values[middleIndex - 1]) / 2;

        var result = new ResultEntity
        {
            Filename = filename,
            DateDeltaSeconds = (maxDate - minDate).TotalSeconds,
            FirstOperationDate = minDate,
            AverageExecutionTime = averageExecutionTime,
            AverageValue = averageValue,
            MedianValue = medianValue,
            MaxValue = maxValue,
            MinValue = minValue,
            Values = rows
        };

        var existingResult = await dbContext.Results
            .Include(existing => existing.Values)
            .SingleOrDefaultAsync(existing => existing.Filename == filename, cancellationToken);

        if (existingResult is not null)
        {
            dbContext.Results.Remove(existingResult);
        }

        dbContext.Results.Add(result);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static FileLine ParseLine(
        string line,
        int lineNumber,
        DateTimeOffset now)
    {
        if (line.Length == 0)
        {
            throw new InvalidDataException($"Line {lineNumber}: empty line.");
        }

        var firstSeparator = line.IndexOf(';');
        var secondSeparator = firstSeparator < 0 ? -1 : line.IndexOf(';', firstSeparator + 1);
        var thirdSeparator = secondSeparator < 0 ? -1 : line.IndexOf(';', secondSeparator + 1);

        if (firstSeparator < 0 || secondSeparator < 0 || thirdSeparator >= 0)
        {
            throw new InvalidDataException($"Line {lineNumber}: expected Date;ExecutionTime;Value format.");
        }

        var span = line.AsSpan();
        var dateSpan = span[..firstSeparator];
        var executionTimeSpan = span[(firstSeparator + 1)..secondSeparator];
        var valueSpan = span[(secondSeparator + 1)..];

        if (!DateTimeOffset.TryParseExact(dateSpan, DateFormat, CultureInfo.InvariantCulture, DateStyle, out var date))
        {
            throw new InvalidDataException($"Line {lineNumber}: invalid Date value.");
        }
        if (date < MinAllowedDate || date > now)
        {
            throw new InvalidDataException($"Line {lineNumber}: Date value out of range.");
        }

        if (!double.TryParse(executionTimeSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out var executionTime))
        {
            throw new InvalidDataException($"Line {lineNumber}: invalid ExecutionTime value.");
        }
        if (!double.IsFinite(executionTime) || executionTime < 0)
        {
            throw new InvalidDataException($"Line {lineNumber}: invalid ExecutionTime value.");
        }

        if (!double.TryParse(valueSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new InvalidDataException($"Line {lineNumber}: invalid Value value.");
        }
        if (!double.IsFinite(value) || value < 0)
        {
            throw new InvalidDataException($"Line {lineNumber}: invalid Value value.");
        }

        return new FileLine(date, executionTime, value);
    }

    private static void ValidateArguments(string filename, Stream data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        ArgumentNullException.ThrowIfNull(data);

        if (!data.CanRead)
        {
            throw new ArgumentException("Stream is not readable.", nameof(data));
        }
    }

    private readonly record struct FileLine(
        DateTimeOffset Date,
        double ExecutionTime,
        double Value);
}
