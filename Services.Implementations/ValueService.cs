using Services.Interfaces;
using Services.Interfaces.Models;

namespace Services.Implementations;

public sealed class ValueService : IValueService
{
    public Task<IReadOnlyList<ValueModel>> GetLatestAsync(
        string filename,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ValueModel>>([]);
    }
}
