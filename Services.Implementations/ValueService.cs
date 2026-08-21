using Data;

using Microsoft.EntityFrameworkCore;

using Services.Interfaces;
using Services.Interfaces.Models;

namespace Services.Implementations;

public sealed class ValueService(AppDbContext dbContext) : IValueService
{
    private const int LatestValuesCount = 10;

    public async Task<IReadOnlyList<ValueModel>> GetLatestAsync(
        string filename,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        return await dbContext.Results
            .AsNoTracking()
            .Where(result => result.Filename == filename)
            .SelectMany(result => result.Values)
            .OrderByDescending(value => value.Date)
            .Take(LatestValuesCount)
            .Select(value => new ValueModel(
                value.Date,
                value.ExecutionTime,
                value.Value))
            .ToListAsync(cancellationToken);
    }
}
