using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Users.GetUserAuditLog;

public record GetUserAuditLogQuery(Guid? TargetUserId = null) : IRequest<IReadOnlyList<UserAuditLogResponse>>;

public class GetUserAuditLogQueryHandler : IRequestHandler<GetUserAuditLogQuery, IReadOnlyList<UserAuditLogResponse>>
{
    private readonly IUserAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public GetUserAuditLogQueryHandler(IUserAuditLogRepository auditLogRepository, IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<UserAuditLogResponse>> Handle(
        GetUserAuditLogQuery request,
        CancellationToken cancellationToken)
    {
        var logs = request.TargetUserId.HasValue
            ? await _auditLogRepository.GetByTargetUserAsync(request.TargetUserId.Value, cancellationToken)
            : await _auditLogRepository.GetAllWithDetailsAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<UserAuditLogResponse>>(logs);
    }
}
