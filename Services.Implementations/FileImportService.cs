using Services.Interfaces;

namespace Services.Implementations;

public sealed class FileImportService : IFileImportService
{
    public Task ImportAsync(
        string filename,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
