using Services.Interfaces.Models;

namespace Services.Interfaces;

public interface IValueService
{
    Task<IReadOnlyList<ValueModel>> GetLatestAsync(
        string filename,
        CancellationToken cancellationToken = default);
}
