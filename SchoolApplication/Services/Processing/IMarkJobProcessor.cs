namespace SchoolApplication.Services.Processing;

public interface IMarkJobProcessor
{
    /// <summary>Claims and processes at most one pending job, if any.</summary>
    Task ProcessNextAsync(CancellationToken cancellationToken = default);
}
