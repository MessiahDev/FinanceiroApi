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
                s.Id, s.FirstName, s.LastName, s.FullName,
                s.Email.Value, s.Cpf.Value, s.Position, s.DepartmentId,
                s.Department != null ? s.Department.Name : string.Empty,
                s.BaseSalary.Amount, s.BaseSalary.Currency,
                s.Status.ToString(), s.ContractType.ToString(),
                s.HireDate, s.TerminationDate, s.CreatedAt, s.UpdatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Employee, EmployeeSummaryResponse>()
            .ConstructUsing(s => new EmployeeSummaryResponse(
                s.Id, s.FullName, s.Position, s.DepartmentId,
                s.Department != null ? s.Department.Name : string.Empty,
                s.BaseSalary.Amount, s.Status.ToString()))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Department, DepartmentResponse>()
            .ConstructUsing(s => new DepartmentResponse(
                s.Id, s.Name, s.Description, s.CostCenter, s.IsActive, s.Employees.Count))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Payroll, PayrollResponse>()
            .ConstructUsing(s => new PayrollResponse(
                s.Id, s.Period.Start.Month, s.Period.Start.Year, s.Period.ToString(),
                s.Status.ToString(), s.TotalGross.Amount, s.TotalDiscounts.Amount,
                s.TotalNet.Amount, s.Items.Count, s.ProcessedAt, s.PaidAt, s.CreatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<PayrollItem, PayrollItemResponse>()
            .ConstructUsing(s => new PayrollItemResponse(
                s.Id, s.EmployeeId,
                s.Employee != null ? s.Employee.FullName : string.Empty,
                s.GrossSalary.Amount, s.InssDiscount.Amount,
                s.IrpfDiscount.Amount, s.OtherDiscounts.Amount, s.NetSalary.Amount))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Payroll, PayrollDetailResponse>()
            .ConstructUsing(s => new PayrollDetailResponse(
                s.Id, s.Period.Start.Month, s.Period.Start.Year, s.Period.ToString(),
                s.Status.ToString(), s.TotalGross.Amount, s.TotalDiscounts.Amount,
                s.TotalNet.Amount, s.Notes, s.ProcessedAt, s.PaidAt, s.CreatedAt,
                s.Items.Select(i => new PayrollItemResponse(
                    i.Id, i.EmployeeId,
                    i.Employee != null ? i.Employee.FullName : string.Empty,
                    i.GrossSalary.Amount, i.InssDiscount.Amount,
                    i.IrpfDiscount.Amount, i.OtherDiscounts.Amount,
                    i.NetSalary.Amount)).ToList().AsReadOnly()))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Transaction, TransactionResponse>()
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.Amount.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Amount.Currency))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<Customer, CustomerResponse>()
            .ConstructUsing(s => new CustomerResponse(
                s.Id, s.Name, s.TaxId, s.PersonType.ToString(), s.Email.Value,
                s.Phone, s.ContactName, s.Status.ToString(),
                s.CreditLimit.Amount, s.CreditLimit.Currency, s.CreatedAt, s.UpdatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Customer, CustomerSummaryResponse>()
            .ConstructUsing(s => new CustomerSummaryResponse(
                s.Id, s.Name, s.TaxId, s.PersonType.ToString(),
                s.Status.ToString(), s.CreditLimit.Amount))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Supplier, SupplierResponse>()
            .ConstructUsing(s => new SupplierResponse(
                s.Id, s.Name, s.TaxId, s.PersonType.ToString(), s.Email.Value,
                s.Phone, s.ContactName, s.Status.ToString(),
                s.BankName, s.BankAgency, s.BankAccount, s.PixKey,
                s.CreatedAt, s.UpdatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Supplier, SupplierSummaryResponse>()
            .ConstructUsing(s => new SupplierSummaryResponse(
                s.Id, s.Name, s.TaxId, s.PersonType.ToString(), s.Status.ToString()))
            .ForAllMembers(o => o.Ignore());

        CreateMap<AccountPayable, AccountPayableResponse>()
            .ConstructUsing(s => new AccountPayableResponse(
                s.Id, s.SupplierId,
                s.Supplier != null ? s.Supplier.Name : string.Empty,
                s.CostCenterId,
                s.CostCenter != null ? s.CostCenter.Name : null,
                s.Description, s.TotalAmount.Amount, s.PaidAmount.Amount,
                s.RemainingAmount.Amount, s.TotalAmount.Currency,
                s.DueDate, s.PaymentDate, s.Status.ToString(),
                s.InvoiceNumber, s.Notes, s.CreatedAt, s.UpdatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<AccountReceivable, AccountReceivableResponse>()
            .ConstructUsing(s => new AccountReceivableResponse(
                s.Id, s.CustomerId,
                s.Customer != null ? s.Customer.Name : string.Empty,
                s.CostCenterId,
                s.CostCenter != null ? s.CostCenter.Name : null,
                s.Description, s.TotalAmount.Amount, s.ReceivedAmount.Amount,
                s.RemainingAmount.Amount, s.TotalAmount.Currency,
                s.DueDate, s.ReceiptDate, s.Status.ToString(),
                s.InvoiceNumber, s.Notes, s.CreatedAt, s.UpdatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<BankAccount, BankAccountResponse>()
            .ConstructUsing(s => new BankAccountResponse(
                s.Id, s.BankName, s.BankCode, s.Agency, s.AccountNumber,
                s.AccountType.ToString(), s.PixKey, s.Balance.Amount,
                s.Balance.Currency, s.IsActive, s.Description,
                s.CreatedAt, s.UpdatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<CostCenter, CostCenterResponse>()
            .ConstructUsing(s => new CostCenterResponse(
                s.Id, s.Code, s.Name, s.Description, s.ParentId,
                s.Parent != null ? s.Parent.Name : null,
                s.AnnualBudget.Amount, s.AnnualBudget.Currency,
                s.Status.ToString(), s.ManagerId,
                s.Manager != null ? s.Manager.FullName : null,
                s.CreatedAt, s.UpdatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<BudgetItem, BudgetItemResponse>()
            .ConstructUsing(s => new BudgetItemResponse(
                s.Id, s.CostCenterId,
                s.CostCenter != null ? s.CostCenter.Name : string.Empty,
                s.Category.ToString(), s.PlannedAmount.Amount,
                s.RealizedAmount.Amount, s.Variance.Amount, s.IsOverBudget))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Budget, BudgetSummaryResponse>()
            .ConstructUsing(s => new BudgetSummaryResponse(
                s.Id, s.Year, s.Name, s.Status.ToString(),
                s.TotalPlanned.Amount, s.TotalRealized.Amount,
                s.Variance.Amount, s.ApprovedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Budget, BudgetResponse>()
            .ConstructUsing(s => new BudgetResponse(
                s.Id, s.Year, s.Name, s.Description, s.Status.ToString(),
                s.TotalPlanned.Amount, s.TotalRealized.Amount, s.Variance.Amount,
                s.TotalPlanned.Currency, s.ApprovedAt, s.ApprovedBy,
                s.CreatedAt, s.UpdatedAt,
                s.Items.Select(i => new BudgetItemResponse(
                    i.Id, i.CostCenterId,
                    i.CostCenter != null ? i.CostCenter.Name : string.Empty,
                    i.Category.ToString(), i.PlannedAmount.Amount,
                    i.RealizedAmount.Amount, i.Variance.Amount,
                    i.IsOverBudget)).ToList().AsReadOnly()))
            .ForAllMembers(o => o.Ignore());

        CreateMap<ChartOfAccount, ChartOfAccountSummaryResponse>()
            .ConstructUsing(src => new ChartOfAccountSummaryResponse(
                src.Id,
                src.Code,
                src.Name,
                src.AccountType,
                src.AccountNature,
                src.AcceptsEntries,
                src.IsActive));

        CreateMap<ChartOfAccount, ChartOfAccountResponse>()
            .ConstructUsing((src, ctx) => new ChartOfAccountResponse(
                src.Id,
                src.Code,
                src.Name,
                src.Description,
                src.AccountType,
                src.AccountType.ToString(),
                src.AccountNature,
                src.AccountNature.ToString(),
                src.AcceptsEntries,
                src.IsActive,
                src.ParentAccountId,
                src.ParentAccount?.Code,
                src.ParentAccount?.Name,
                src.ChildAccounts.Any()
                    ? ctx.Mapper.Map<IEnumerable<ChartOfAccountResponse>>(src.ChildAccounts)
                    : null,
                src.CreatedAt,
                src.UpdatedAt));

        CreateMap<JournalEntryLine, JournalEntryLineResponse>()
            .ConstructUsing(src => new JournalEntryLineResponse(
                src.Id,
                src.ChartOfAccountId,
                src.ChartOfAccount != null ? src.ChartOfAccount.Code : string.Empty,
                src.ChartOfAccount != null ? src.ChartOfAccount.Name : string.Empty,
                src.DebitCredit,
                src.DebitCredit.ToString(),
                src.Amount,
                src.Description,
                src.LineOrder));

        CreateMap<JournalEntry, JournalEntrySummaryResponse>()
            .ConstructUsing(src => new JournalEntrySummaryResponse(
                src.Id,
                src.EntryNumber,
                src.Description,
                src.EntryDate,
                src.Status,
                src.Status.ToString(),
                src.EntryType,
                src.TotalDebits()));

        CreateMap<JournalEntry, JournalEntryResponse>()
            .ConstructUsing((src, ctx) => new JournalEntryResponse(
                src.Id,
                src.EntryNumber,
                src.Description,
                src.EntryDate,
                src.Status,
                src.Status.ToString(),
                src.EntryType,
                src.EntryType.ToString(),
                src.ReferenceDocument,
                src.ReferenceDocumentType,
                src.ReferenceDocumentId,
                src.AccountingPeriodId,
                src.AccountingPeriod != null ? src.AccountingPeriod.Name : string.Empty,
                src.TotalDebits(),
                src.TotalCredits(),
                src.IsBalanced(),
                ctx.Mapper.Map<IEnumerable<JournalEntryLineResponse>>(src.Lines),
                src.CreatedAt,
                src.UpdatedAt));

        CreateMap<AccountingPeriod, AccountingPeriodResponse>()
            .ConstructUsing(src => new AccountingPeriodResponse(
                src.Id,
                src.Name,
                src.Year,
                src.Month,
                src.Period.Start.ToDateTime(TimeOnly.MinValue),
                src.Period.End.ToDateTime(TimeOnly.MinValue),
                src.Status,
                src.Status.ToString(),
                src.JournalEntries.Count,
                src.CreatedAt,
                src.UpdatedAt));
    }
}

