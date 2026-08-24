using Data.Entities;

using Services.Implementations;
using Services.Interfaces.Models;

namespace Tests.Services.Implementations;

public sealed class ResultServiceTests(PostgreSqlFixture database) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task SearchAsync_ThrowsArgumentNullException_WhenFilterIsNull()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new ResultService(dbContext);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SearchAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyResult_WhenNothingFound()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new ResultService(dbContext);

        var result = await service.SearchAsync(new ResultSearchFilter(), TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_ReturnsResult()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new ResultService(dbContext);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        var date = new DateTimeOffset(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

        dbContext.Results.Add(new ResultEntity
        {
            Filename = "file.csv",
            DateDeltaSeconds = 10.1,
            FirstOperationDate = date,
            AverageExecutionTime = 20.2,
            AverageValue = 30.3,
            MedianValue = 40.4,
            MaxValue = 50.5,
            MinValue = 5.5
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await service.SearchAsync(new ResultSearchFilter
        {
            Filename = "file.csv"
        }, TestContext.Current.CancellationToken);

        Assert.Single(result);
        Assert.Equal(new ResultModel("file.csv", 10.1, date, 20.2, 30.3, 40.4, 50.5, 5.5), result[0]);
    }

    [Fact]
    public async Task SearchAsync_AppliesFilters()
    {
        await using var dbContext = database.CreateDbContext();
        var service = new ResultService(dbContext);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        var date = new DateTimeOffset(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

        dbContext.Results.AddRange(
            new ResultEntity
            {
                Filename = "1.csv",
                FirstOperationDate = date,
                AverageValue = 10,
                AverageExecutionTime = 1
            },
            new ResultEntity
            {
                Filename = "2.csv",
                FirstOperationDate = date.AddYears(1),
                AverageValue = 20,
                AverageExecutionTime = 2
            },
            new ResultEntity
            {
                Filename = "3.csv",
                FirstOperationDate = date.AddYears(2),
                AverageValue = 30,
                AverageExecutionTime = 3
            });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var filenameResult = await service.SearchAsync(new ResultSearchFilter
        {
            Filename = "2.csv"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(["2.csv"], filenameResult.Select(result => result.Filename).Order(StringComparer.Ordinal));

        var dateFromResult = await service.SearchAsync(new ResultSearchFilter
        {
            FirstOperationDateFrom = date.AddYears(1)
        }, TestContext.Current.CancellationToken);
        Assert.Equal(["2.csv", "3.csv"], dateFromResult.Select(result => result.Filename).Order(StringComparer.Ordinal));

        var dateToResult = await service.SearchAsync(new ResultSearchFilter
        {
            FirstOperationDateTo = date.AddYears(1)
        }, TestContext.Current.CancellationToken);
        Assert.Equal(["1.csv", "2.csv"], dateToResult.Select(result => result.Filename).Order(StringComparer.Ordinal));

        var averageValueFromResult = await service.SearchAsync(new ResultSearchFilter
        {
            AverageValueFrom = 20
        }, TestContext.Current.CancellationToken);
        Assert.Equal(["2.csv", "3.csv"], averageValueFromResult.Select(result => result.Filename).Order(StringComparer.Ordinal));

        var averageValueToResult = await service.SearchAsync(new ResultSearchFilter
        {
            AverageValueTo = 20
        }, TestContext.Current.CancellationToken);
        Assert.Equal(["1.csv", "2.csv"], averageValueToResult.Select(result => result.Filename).Order(StringComparer.Ordinal));

        var averageExecutionTimeFromResult = await service.SearchAsync(new ResultSearchFilter
        {
            AverageExecutionTimeFrom = 2
        }, TestContext.Current.CancellationToken);
        Assert.Equal(["2.csv", "3.csv"], averageExecutionTimeFromResult.Select(result => result.Filename).Order(StringComparer.Ordinal));

        var averageExecutionTimeToResult = await service.SearchAsync(new ResultSearchFilter
        {
            AverageExecutionTimeTo = 2
        }, TestContext.Current.CancellationToken);
        Assert.Equal(["1.csv", "2.csv"], averageExecutionTimeToResult.Select(result => result.Filename).Order(StringComparer.Ordinal));

        var allResult = await service.SearchAsync(new ResultSearchFilter
        {
            Filename = "2.csv",
            FirstOperationDateFrom = date.AddYears(1),
            FirstOperationDateTo = date.AddYears(1),
            AverageValueFrom = 20,
            AverageValueTo = 20,
            AverageExecutionTimeFrom = 2,
            AverageExecutionTimeTo = 2
        }, TestContext.Current.CancellationToken);
        Assert.Equal(["2.csv"], allResult.Select(result => result.Filename).Order(StringComparer.Ordinal));
    }
}
