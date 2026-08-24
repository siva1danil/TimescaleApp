using System.Globalization;
using System.Text;

using Microsoft.EntityFrameworkCore;

using Services.Implementations;
using Services.Interfaces.Exceptions;

namespace Tests.Services.Implementations;

public sealed class FileImportServiceTests(PostgreSqlFixture database) : IClassFixture<PostgreSqlFixture>
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ImportAsync_ThrowsArgumentException_WhenFilenameIsInvalid(string? filename)
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.ImportAsync(filename!, Stream.Null));
    }

    [Fact]
    public async Task ImportAsync_ThrowsArgumentNullException_WhenStreamIsNull()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.ImportAsync("file.csv", null!));
    }

    [Fact]
    public async Task ImportAsync_ThrowsArgumentException_WhenStreamIsNotReadable()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        var stream = new MemoryStream();
        await stream.DisposeAsync();

        await Assert.ThrowsAsync<ArgumentException>(() => service.ImportAsync("file.csv", stream));
    }

    [Theory]
    [InlineData("", "CSV header")]
    [InlineData("Wrong;Header;Names\n2020-01-01T00-00-00.0000Z;1;2", "CSV header")]
    [InlineData("Date;ExecutionTime;Value", "at least one data row")]
    [InlineData("Date;ExecutionTime;Value\n\n", "Line 2: empty line")]
    [InlineData("Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;1", "expected Date;ExecutionTime;Value format")]
    [InlineData("Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;1;2;3", "expected Date;ExecutionTime;Value format")]
    [InlineData("Date;ExecutionTime;Value\ninvalid;1;2", "invalid 'Date' value")]
    [InlineData("Date;ExecutionTime;Value\n1999-12-31T23-59-59.9999Z;1;2", "'Date' value out of range")]
    [InlineData("Date;ExecutionTime;Value\n2099-01-01T00-00-00.0000Z;1;2", "'Date' value out of range")]
    [InlineData("Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;invalid;2", "invalid 'ExecutionTime' value")]
    [InlineData("Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;-1;2", "'ExecutionTime' value out of range")]
    [InlineData("Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;NaN;2", "'ExecutionTime' value out of range")]
    [InlineData("Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;1;invalid", "invalid 'Value' value")]
    [InlineData("Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;1;-1", "'Value' value out of range")]
    [InlineData("Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;1;Infinity", "'Value' value out of range")]
    public async Task ImportAsync_ThrowsCsvValidationException_WhenCsvIsInvalid(
        string csv,
        string expectedMessage)
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);
        var filename = $"{Guid.NewGuid()}.csv";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var exception = await Assert.ThrowsAsync<CsvValidationException>(() => service.ImportAsync(filename, stream));
        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
        Assert.False(await dbContext.Results.AnyAsync(result => result.Filename == filename));
    }

    [Fact]
    public async Task ImportAsync_SavesValuesCorrectly()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        var csv = "Date;ExecutionTime;Value\n2020-01-01T00-00-30.0000Z;1;10\n2020-01-01T00-00-00.0000Z;2;2\n2020-01-01T00-00-20.0000Z;3;8\n2020-01-01T00-00-10.0000Z;4;4";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        await service.ImportAsync("values.csv", stream);
        var result = await dbContext.Results
            .AsNoTracking()
            .Include(item => item.Values)
            .SingleAsync(item => item.Filename == "values.csv");

        Assert.Equal(30, result.DateDeltaSeconds);
        Assert.Equal(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), result.FirstOperationDate);
        Assert.Equal(2.5, result.AverageExecutionTime);
        Assert.Equal(6, result.AverageValue);
        Assert.Equal(6, result.MedianValue);
        Assert.Equal(10, result.MaxValue);
        Assert.Equal(2, result.MinValue);
        Assert.Equal(4, result.Values.Count);
        Assert.Equal([2.0, 4.0, 8.0, 10.0], result.Values.Select(value => value.Value).Order());
    }

    [Fact]
    public async Task ImportAsync_CalculatesMedian_WhenCountOdd()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        var csv = "Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;1;100\n2020-01-01T00-00-01.0000Z;1;1\n2020-01-01T00-00-02.0000Z;1;7";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        await service.ImportAsync("odd-median.csv", stream);
        var result = await dbContext.Results
            .AsNoTracking()
            .SingleAsync(item => item.Filename == "odd-median.csv");

        Assert.Equal(7, result.MedianValue);
    }

    [Fact]
    public async Task ImportAsync_CalculatesAggregates_WhenSumOverflows()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        var doubleMaxValueStr = double.MaxValue.ToString("R", CultureInfo.InvariantCulture);
        var csv = $"Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;{doubleMaxValueStr};{doubleMaxValueStr}\n2020-01-01T00-00-01.0000Z;{doubleMaxValueStr};{doubleMaxValueStr}";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        await service.ImportAsync("overflow.csv", stream);
        var result = await dbContext.Results
            .AsNoTracking()
            .SingleAsync(item => item.Filename == "overflow.csv");

        Assert.Equal(double.MaxValue, result.AverageExecutionTime);
        Assert.Equal(double.MaxValue, result.AverageValue);
        Assert.Equal(double.MaxValue, result.MedianValue);
        Assert.True(double.IsFinite(result.AverageExecutionTime));
        Assert.True(double.IsFinite(result.AverageValue));
        Assert.True(double.IsFinite(result.MedianValue));
    }

    [Fact]
    public async Task ImportAsync_ReplacesExisting()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        var csv1 = "Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;1;1\n2020-01-01T00-00-01.0000Z;2;2";
        var csv2 = "Date;ExecutionTime;Value\n2020-01-01T00-00-02.0000Z;9;9";
        await using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(csv1));
        await using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(csv2));

        await service.ImportAsync("replacement.csv", stream1);
        await service.ImportAsync("replacement.csv", stream2);

        var result = await dbContext.Results
            .AsNoTracking()
            .Include(item => item.Values)
            .SingleAsync(item => item.Filename == "replacement.csv");

        var value = Assert.Single(result.Values);
        Assert.Equal(9, value.ExecutionTime);
        Assert.Equal(9, value.Value);
        Assert.Equal(1, await dbContext.Results.CountAsync(item => item.Filename == "replacement.csv"));
    }

    [Fact]
    public async Task ImportAsync_DoesNotReplace_WhenNewFileInvalid()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        var csv1 = "Date;ExecutionTime;Value\n2020-01-01T00-00-00.0000Z;1;42";
        await using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(csv1));
        await using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("Date;ExecutionTime;Value\ninvalid"));

        await service.ImportAsync("preserved.csv", stream1);
        await Assert.ThrowsAsync<CsvValidationException>(() => service.ImportAsync("preserved.csv", stream2));
        var result = await dbContext.Results
            .AsNoTracking()
            .Include(item => item.Values)
            .SingleAsync(item => item.Filename == "preserved.csv");

        var value = Assert.Single(result.Values);
        Assert.Equal(42, value.Value);
    }

    [Fact]
    public async Task ImportAsync_AcceptsMaxRows()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        var date = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var rows = Enumerable.Range(0, 10000)
            .Select(index => $"{date.AddSeconds(index):yyyy-MM-dd'T'HH-mm-ss.ffff'Z'};1;1");
        var csv = string.Join('\n', rows.Prepend("Date;ExecutionTime;Value"));
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        await service.ImportAsync("10000.csv", stream);

        var resultId = await dbContext.Results
            .Where(result => result.Filename == "10000.csv")
            .Select(result => result.Id)
            .SingleAsync();
        Assert.Equal(10000, await dbContext.Values.CountAsync(value => value.ResultId == resultId));
    }

    [Fact]
    public async Task ImportAsync_RejectsMaxRowsPlusOne()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new FileImportService(dbContext, TimeProvider.System);

        var date = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var rows = Enumerable.Range(0, 10001)
            .Select(index => $"{date.AddSeconds(index):yyyy-MM-dd'T'HH-mm-ss.ffff'Z'};1;1");
        var csv = string.Join('\n', rows.Prepend("Date;ExecutionTime;Value"));
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        await Assert.ThrowsAsync<CsvValidationException>(() => service.ImportAsync("10001.csv", stream));
        Assert.False(await dbContext.Results.AnyAsync(result => result.Filename == "10001.csv"));
    }

    [Fact]
    public async Task ImportAsync_WorksConcurrently()
    {
        var tasks = Enumerable.Range(1, 20)
            .Select(async value =>
            {
                await using var dbContext = database.CreateDbContext();
                var service = new FileImportService(dbContext, TimeProvider.System);

                var csv = string.Join('\n', "Date;ExecutionTime;Value", $"2020-01-01T00-00-00.0000Z;{value};{value}");
                await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

                await service.ImportAsync("concurrent.csv", stream);
            });
        await Task.WhenAll(tasks);

        await using var dbContext = database.CreateDbContext();
        var result = await dbContext.Results
            .AsNoTracking()
            .Include(item => item.Values)
            .SingleAsync(item => item.Filename == "concurrent.csv");

        Assert.Single(result.Values);
        Assert.InRange(result.Values.Single().Value, 1, 20);
    }
}