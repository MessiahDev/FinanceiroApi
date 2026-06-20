using FinanceiroApi.Domain.Entities;

namespace FinanceiroApi.Domain.Interfaces.Repositories;

public interface IUserAuditLogRepository : IRepositoryBase<UserAuditLog>
{
    Task<IReadOnlyList<UserAuditLog>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UserAuditLog>> GetByTargetUserAsync(Guid targetUserId, CancellationToken ct = default);
}
