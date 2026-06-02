using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.Employees.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeResponse?>;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponse?>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetEmployeeByIdQueryHandler(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        IMapper mapper,
        ICacheService cache)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<EmployeeResponse?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"employee:{request.Id}";
        var cached = await _cache.GetAsync<EmployeeResponse>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null) return null;

        var department = await _departmentRepository.GetByIdAsync(employee.DepartmentId, cancellationToken);
        var response = _mapper.Map<EmployeeResponse>(employee) with { DepartmentName = department?.Name ?? string.Empty };

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);
        return response;
    }
}
