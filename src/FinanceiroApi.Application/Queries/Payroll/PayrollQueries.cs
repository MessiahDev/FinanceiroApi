using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Payroll.GetPayrollById
{
    public record GetPayrollByIdQuery(Guid Id) : IRequest<PayrollDetailResponse?>;

    public class GetPayrollByIdQueryHandler : IRequestHandler<GetPayrollByIdQuery, PayrollDetailResponse?>
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IMapper _mapper;

        public GetPayrollByIdQueryHandler(IPayrollRepository payrollRepository, IMapper mapper)
        {
            _payrollRepository = payrollRepository;
            _mapper = mapper;
        }

        public async Task<PayrollDetailResponse?> Handle(GetPayrollByIdQuery request, CancellationToken cancellationToken)
        {
            var payroll = await _payrollRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
            if (payroll is null) return null;

            return _mapper.Map<PayrollDetailResponse>(payroll.Items.FirstOrDefault());
        }
    }
}

namespace FinanceiroApi.Application.Queries.Payroll.GetPayrollHistory
{
    public record GetPayrollHistoryQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<PayrollResponse>>;

    public class GetPayrollHistoryQueryHandler : IRequestHandler<GetPayrollHistoryQuery, PagedResult<PayrollResponse>>
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IMapper _mapper;

        public GetPayrollHistoryQueryHandler(IPayrollRepository payrollRepository, IMapper mapper)
        {
            _payrollRepository = payrollRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<PayrollResponse>> Handle(GetPayrollHistoryQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _payrollRepository.GetHistoryPagedAsync(request.Page, request.PageSize, cancellationToken);
            var dtos = _mapper.Map<List<PayrollResponse>>(items);
            return new PagedResult<PayrollResponse>(dtos, total, request.Page, request.PageSize);
        }
    }
}
