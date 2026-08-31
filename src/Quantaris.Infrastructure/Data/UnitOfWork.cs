namespace Quantaris.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    public QuantarisDbContext Context { get; }

    public UnitOfWork(QuantarisDbContext context) => Context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);

    public void Dispose() => Context.Dispose();
}
