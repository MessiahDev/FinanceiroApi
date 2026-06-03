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

        CreateMap<PayrollItem, PayrollItemResponse>()
            .ConstructUsing(s => new PayrollItemResponse(
                s.Id,
                s.EmployeeId,
                s.Employee != null ? s.Employee.FullName : string.Empty,
                s.GrossSalary.Amount,
                s.InssDiscount.Amount,
                s.IrpfDiscount.Amount,
                s.OtherDiscounts.Amount,
                s.NetSalary.Amount));

        CreateMap<Payroll, PayrollDetailResponse>()
            .ConstructUsing(s => new PayrollDetailResponse(
                s.Id,
                s.Period.Start.Month,
                s.Period.Start.Year,
                s.Period.ToString(),
                s.Status.ToString(),
                s.TotalGross.Amount,
                s.TotalDiscounts.Amount,
                s.TotalNet.Amount,
                s.Notes,
                s.ProcessedAt,
                s.PaidAt,
                s.CreatedAt,
                s.Items.Select(i => new PayrollItemResponse(
                    i.Id,
                    i.EmployeeId,
                    i.Employee != null ? i.Employee.FullName : string.Empty,
                    i.GrossSalary.Amount,
                    i.InssDiscount.Amount,
                    i.IrpfDiscount.Amount,
                    i.OtherDiscounts.Amount,
                    i.NetSalary.Amount)).ToList().AsReadOnly()))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Transaction, TransactionResponse>()
            .ForMember(d => d.Amount,           o => o.MapFrom(s => s.Amount.Amount))
            .ForMember(d => d.Currency,         o => o.MapFrom(s => s.Amount.Currency))
            .ForMember(d => d.Type,             o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Category,         o => o.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Status,           o => o.MapFrom(s => s.Status.ToString()));
    }
}


