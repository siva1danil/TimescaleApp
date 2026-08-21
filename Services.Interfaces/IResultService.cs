using Services.Interfaces.Models;

namespace Services.Interfaces;

public interface IResultService
{
    Task<IReadOnlyList<ResultModel>> SearchAsync(
        ResultSearchFilter filter,
        CancellationToken cancellationToken = default);
}
