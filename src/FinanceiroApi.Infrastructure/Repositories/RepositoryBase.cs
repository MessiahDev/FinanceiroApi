using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity> : IRepositoryBase<TEntity>
    where TEntity : BaseEntity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected RepositoryBase(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = Context.Entry(entity);
        if (entry.State == EntityState.Detached)
            DbSet.Update(entity);

        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(e => e.Id == id, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await Context.SaveChangesAsync(cancellationToken);
}
