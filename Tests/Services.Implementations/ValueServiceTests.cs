using Data.Entities;

using Services.Implementations;
using Services.Interfaces.Models;

namespace Tests.Services.Implementations;

public sealed class ValueServiceTests(PostgreSqlFixture database) : IClassFixture<PostgreSqlFixture>
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetLatestAsync_ThrowsArgumentException_WhenFilenameIsInvalid(string? filename)
    {
        await using var dbContext = database.CreateDbContext();
        var service = new ValueService(dbContext);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.GetLatestAsync(filename!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsEmptyResult_WhenFileDoesNotExist()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new ValueService(dbContext);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        var result = await service.GetLatestAsync("file.csv", TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsOnlyCorrectFileValues()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new ValueService(dbContext);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        var date = new DateTimeOffset(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

        dbContext.Results.AddRange(
            new ResultEntity
            {
                Filename = "1.csv",
                Values =
                [
                    new ValueEntity
                    {
                        Date = date,
                        ExecutionTime = 12.3,
                        Value = 45.6
                    }
                ]
            },
            new ResultEntity
            {
                Filename = "2.csv",
                Values =
                [
                    new ValueEntity
                    {
                        Date = date.AddDays(1),
                        ExecutionTime = 100,
                        Value = 200
                    }
                ]
            });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await service.GetLatestAsync("1.csv", TestContext.Current.CancellationToken);

        Assert.Single(result);
        Assert.Equal(new ValueModel(date, 12.3, 45.6), result[0]);
    }

    [Theory]
    [InlineData(new int[] { 5, 0, 11, 3, 8, 1, 10, 6, 4, 9, 2, 7 }, new int[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 })]
    [InlineData(new int[] { 3, 9, 0, 8, 1, 7, 2, 6, 4, 5 }, new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 })]
    [InlineData(new int[] { 14, 2, 8, 0, 13, 5, 10, 3, 12, 6, 1, 11, 4, 9, 7 }, new int[] { 14, 13, 12, 11, 10, 9, 8, 7, 6, 5 })]
    public async Task GetLatestAsync_ReturnsValuesDescending(int[] input, int[] expected)
    {
        await using var dbContext = database.CreateDbContext();
        var service = new ValueService(dbContext);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        var date = new DateTimeOffset(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);
        var values = input
            .Select(value => new ValueEntity
            {
                Date = date.AddMinutes(value),
                ExecutionTime = value + 1.0,
                Value = value
            })
            .ToArray();

        dbContext.Results.Add(new ResultEntity
        {
            Filename = "file.csv",
            Values = values
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await service.GetLatestAsync("file.csv", TestContext.Current.CancellationToken);

        Assert.Equal(expected.Length, result.Count);
        Assert.Equal(expected.Select(value => (double)value), result.Select(value => value.Value));
        Assert.Equal(expected.Select(value => date.AddMinutes(value)), result.Select(value => value.Date));
        Assert.Equal(expected.Select(value => value + 1.0), result.Select(value => value.ExecutionTime));
    }
}
