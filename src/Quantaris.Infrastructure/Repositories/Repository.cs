using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Quantaris.Infrastructure.Data;

namespace Quantaris.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly QuantarisDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(QuantarisDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual void Update(T entity) => _dbSet.Update(entity);

    public virtual void Delete(T entity) => _dbSet.Remove(entity);

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate is null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

    public IQueryable<T> Query() => _dbSet;
}
