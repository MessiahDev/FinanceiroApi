using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Employees.GetEmployeesByDepartment;

public record GetEmployeesByDepartmentQuery(
    Guid DepartmentId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<EmployeeSummaryResponse>>;

public class GetEmployeesByDepartmentQueryHandler
    : IRequestHandler<GetEmployeesByDepartmentQuery, PagedResult<EmployeeSummaryResponse>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public GetEmployeesByDepartmentQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<EmployeeSummaryResponse>> Handle(
        GetEmployeesByDepartmentQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            null,
            request.DepartmentId,
            null,
            cancellationToken);

        var dtos = _mapper.Map<List<EmployeeSummaryResponse>>(result.Items);
        return new PagedResult<EmployeeSummaryResponse>(dtos, result.TotalCount, request.Page, request.PageSize);
    }
}
