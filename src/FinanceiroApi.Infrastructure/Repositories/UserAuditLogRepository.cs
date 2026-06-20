using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceiroApi.Infrastructure.Repositories;

public sealed class UserAuditLogRepository : RepositoryBase<UserAuditLog>, IUserAuditLogRepository
{
    public UserAuditLogRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<UserAuditLog>> GetAllWithDetailsAsync(CancellationToken ct = default)
        => await DbSet
            .Include(l => l.TargetUser)
            .Include(l => l.ChangedByUser)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<UserAuditLog>> GetByTargetUserAsync(Guid targetUserId, CancellationToken ct = default)
        => await DbSet
            .Include(l => l.ChangedByUser)
            .Where(l => l.TargetUserId == targetUserId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);
}
