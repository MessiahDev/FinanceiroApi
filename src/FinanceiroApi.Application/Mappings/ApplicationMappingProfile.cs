using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Entities;

namespace FinanceiroApi.Application.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<Employee, EmployeeResponse>()
            .ConstructUsing(s => new EmployeeResponse(
                s.Id,
                s.FirstName,
                s.LastName,
                s.FullName,
                s.Email.Value,
                s.Cpf.Value,
                s.Position,
                s.DepartmentId,
                s.Department != null ? s.Department.Name : string.Empty,
                s.BaseSalary.Amount,
                s.BaseSalary.Currency,
                s.Status.ToString(),
                s.ContractType.ToString(),
                s.HireDate,
                s.TerminationDate,
                s.CreatedAt,
                s.UpdatedAt));

        CreateMap<Employee, EmployeeSummaryResponse>()
            .ConstructUsing(s => new EmployeeSummaryResponse(
                s.Id,
                s.FullName,
                s.Position,
                s.DepartmentId,
                s.Department != null ? s.Department.Name : string.Empty,
                s.BaseSalary.Amount,
                s.Status.ToString()));

        CreateMap<Department, DepartmentResponse>()
            .ConstructUsing(s => new DepartmentResponse(
                s.Id,
                s.Name,
                s.Description,
                s.CostCenter,
                s.IsActive,
                s.Employees.Count));

        CreateMap<Payroll, PayrollResponse>()
            .ConstructUsing(s => new PayrollResponse(
                s.Id,
                s.Period.Start.Month,
                s.Period.Start.Year,
                s.Period.ToString(),
                s.Status.ToString(),
                s.TotalGross.Amount,
                s.TotalDiscounts.Amount,
                s.TotalNet.Amount,
                s.Items.Count,
                s.ProcessedAt,
                s.PaidAt,
                s.CreatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<PayrollItem, PayrollDetailResponse>()
            .ForMember(d => d.GrossSalary,      o => o.MapFrom(s => s.GrossSalary.Amount))
            .ForMember(d => d.InssDiscount,     o => o.MapFrom(s => s.InssDiscount.Amount))
            .ForMember(d => d.IrpfDiscount,     o => o.MapFrom(s => s.IrpfDiscount.Amount))
            .ForMember(d => d.OtherDiscounts,   o => o.MapFrom(s => s.OtherDiscounts.Amount))
            .ForMember(d => d.NetSalary,        o => o.MapFrom(s => s.NetSalary.Amount))
            .ForMember(d => d.EmployeeName,     o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : string.Empty));

        CreateMap<Transaction, TransactionResponse>()
            .ForMember(d => d.Amount,           o => o.MapFrom(s => s.Amount.Amount))
            .ForMember(d => d.Currency,         o => o.MapFrom(s => s.Amount.Currency))
            .ForMember(d => d.Type,             o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Category,         o => o.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Status,           o => o.MapFrom(s => s.Status.ToString()));
    }
}


