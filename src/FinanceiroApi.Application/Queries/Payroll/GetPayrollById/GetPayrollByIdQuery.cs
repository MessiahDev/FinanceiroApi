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

            return _mapper.Map<PayrollDetailResponse>(payroll);
        }
    }
}