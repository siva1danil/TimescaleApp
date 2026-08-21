using Services.Interfaces;
using Services.Interfaces.Models;

namespace Services.Implementations;

public sealed class ResultService : IResultService
{
    public Task<IReadOnlyList<ResultModel>> SearchAsync(
        ResultSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ResultModel>>([]);
    }
}
