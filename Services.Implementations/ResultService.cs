using Data;

using Microsoft.EntityFrameworkCore;

using Services.Interfaces;
using Services.Interfaces.Models;

namespace Services.Implementations;

public sealed class ResultService(AppDbContext dbContext) : IResultService
{
    public async Task<IReadOnlyList<ResultModel>> SearchAsync(
        ResultSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var results = dbContext.Results.AsNoTracking();

        if (filter.Filename is not null)
        {
            results = results.Where(result => result.Filename == filter.Filename);
        }

        if (filter.FirstOperationDateFrom.HasValue)
        {
            results = results.Where(result => result.FirstOperationDate >= filter.FirstOperationDateFrom.Value);
        }

        if (filter.FirstOperationDateTo.HasValue)
        {
            results = results.Where(result => result.FirstOperationDate <= filter.FirstOperationDateTo.Value);
        }

        if (filter.AverageValueFrom.HasValue)
        {
            results = results.Where(result => result.AverageValue >= filter.AverageValueFrom.Value);
        }

        if (filter.AverageValueTo.HasValue)
        {
            results = results.Where(result => result.AverageValue <= filter.AverageValueTo.Value);
        }

        if (filter.AverageExecutionTimeFrom.HasValue)
        {
            results = results.Where(result => result.AverageExecutionTime >= filter.AverageExecutionTimeFrom.Value);
        }

        if (filter.AverageExecutionTimeTo.HasValue)
        {
            results = results.Where(result => result.AverageExecutionTime <= filter.AverageExecutionTimeTo.Value);
        }

        return await results
            .Select(result => new ResultModel(
                result.Filename,
                result.DateDeltaSeconds,
                result.FirstOperationDate,
                result.AverageExecutionTime,
                result.AverageValue,
                result.MedianValue,
                result.MaxValue,
                result.MinValue))
            .ToListAsync(cancellationToken);
    }
}
