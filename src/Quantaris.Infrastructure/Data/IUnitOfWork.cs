namespace Quantaris.Infrastructure.Data;

public interface IUnitOfWork : IDisposable
{
    QuantarisDbContext Context { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
