namespace Services.Interfaces;

public interface IFileImportService
{
    Task ImportAsync(
        string filename,
        Stream data,
        CancellationToken cancellationToken = default);
}
