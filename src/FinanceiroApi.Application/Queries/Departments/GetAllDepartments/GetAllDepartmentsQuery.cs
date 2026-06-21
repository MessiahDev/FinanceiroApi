using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Departments.GetAllDepartments;

public record GetAllDepartmentsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<DepartmentResponse>>;

public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, PagedResult<DepartmentResponse>>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;

    public GetAllDepartmentsQueryHandler(IDepartmentRepository departmentRepository, IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<DepartmentResponse>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var result = await _departmentRepository.GetActivePagedAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<DepartmentResponse>(
            _mapper.Map<IReadOnlyList<DepartmentResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
