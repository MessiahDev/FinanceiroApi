using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.Employees.GetAllEmployees;
using FinanceiroApi.Application.Queries.Employees.GetEmployeeById;
using FinanceiroApi.Application.Queries.Employees.GetEmployeesByDepartment;
using FinanceiroApi.Application.Queries.JournalEntries.GetJournalEntriesByPeriod;
using FinanceiroApi.Application.Queries.JournalEntries.GetJournalEntryById;
using FinanceiroApi.Application.Queries.Payroll.GetPayrollById;
using FinanceiroApi.Application.Queries.Payroll.GetPayrollHistory;
using FinanceiroApi.Application.Queries.Suppliers.GetAllSuppliers;
using FinanceiroApi.Application.Queries.Suppliers.GetSupplierById;
using FinanceiroApi.Application.Queries.TaxEntries.GetOverdueTaxEntries;
using FinanceiroApi.Application.Queries.TaxEntries.GetTaxEntries;
using FinanceiroApi.Application.Queries.TaxEntries.GetTaxEntryById;
using FinanceiroApi.Application.Queries.TaxPayments.GetTaxPaymentById;
using FinanceiroApi.Application.Queries.TaxPayments.GetTaxPaymentsByEntry;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Queries;

public class GetAllEmployeesQueryHandlerTests
{
    private readonly IEmployeeRepository _repo = Substitute.For<IEmployeeRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAllEmployeesQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ShouldReturnPagedEmployees()
    {
        var pagedResult = new PagedResult<Employee>(new List<Employee>().AsReadOnly(), 0, 1, 20);
        _repo.GetPagedAsync(1, 20, null, null, true, default).Returns(pagedResult);
        _mapper.Map<List<EmployeeSummaryResponse>>(Arg.Any<object>()).Returns(new List<EmployeeSummaryResponse>());
        var result = await CreateHandler().Handle(new GetAllEmployeesQuery(), default);
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }
}

public class GetEmployeeByIdQueryHandlerTests
{
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly IDepartmentRepository _departmentRepo = Substitute.For<IDepartmentRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private GetEmployeeByIdQueryHandler CreateHandler() => new(_employeeRepo, _departmentRepo, _mapper, _cache);

    [Fact]
    public async Task Handle_WithExistingEmployee_ShouldReturnResponse()
    {
        var deptId = Guid.NewGuid();
        var employee = Employee.Create("Joao", "Silva", "52998224725", "joao@empresa.com", 5000m, ContractType.CLT, deptId);
        var department = Department.Create("TI", "TI-001");
        var expected = new EmployeeResponse(employee.Id, "Joao", "Silva", "Joao Silva",
            "joao@empresa.com", "52998224725", null, deptId, "TI", 5000m, "BRL",
            "Active", "CLT", DateOnly.FromDateTime(DateTime.Today), null, DateTime.UtcNow, null);
        _cache.GetAsync<EmployeeResponse>(Arg.Any<string>(), default).ReturnsNull();
        _employeeRepo.GetByIdAsync(employee.Id, default).Returns(employee);
        _departmentRepo.GetByIdAsync(deptId, default).Returns(department);
        _mapper.Map<EmployeeResponse>(employee).Returns(expected);
        var result = await CreateHandler().Handle(new GetEmployeeByIdQuery(employee.Id), default);
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Joao");
    }

    [Fact]
    public async Task Handle_WithNonExistentEmployee_ShouldReturnNull()
    {
        _cache.GetAsync<EmployeeResponse>(Arg.Any<string>(), default).ReturnsNull();
        _employeeRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetEmployeeByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCached_ShouldReturnCachedResponse()
    {
        var cached = new EmployeeResponse(Guid.NewGuid(), "Cache", "Hit", "Cache Hit",
            "a@b.com", "52998224725", null, Guid.NewGuid(), "TI", 5000m, "BRL",
            "Active", "CLT", DateOnly.FromDateTime(DateTime.Today), null, DateTime.UtcNow, null);
        _cache.GetAsync<EmployeeResponse>(Arg.Any<string>(), default).Returns(cached);
        var result = await CreateHandler().Handle(new GetEmployeeByIdQuery(Guid.NewGuid()), default);
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Cache");
        await _employeeRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), default);
    }
}

public class GetEmployeesByDepartmentQueryHandlerTests
{
    private readonly IEmployeeRepository _repo = Substitute.For<IEmployeeRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetEmployeesByDepartmentQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ShouldReturnPagedEmployeesByDepartment()
    {
        var deptId = Guid.NewGuid();
        var pagedResult = new PagedResult<Employee>(new List<Employee>().AsReadOnly(), 0, 1, 20);
        _repo.GetPagedAsync(1, 20, null, deptId, null, default).Returns(pagedResult);
        _mapper.Map<List<EmployeeSummaryResponse>>(Arg.Any<object>()).Returns(new List<EmployeeSummaryResponse>());
        var result = await CreateHandler().Handle(new GetEmployeesByDepartmentQuery(deptId), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(1, 20, null, deptId, null, default);
    }
}

public class GetJournalEntryByIdQueryHandlerTests
{
    private readonly IJournalEntryRepository _repo = Substitute.For<IJournalEntryRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetJournalEntryByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingEntry_ShouldReturnResponse()
    {
        var periodId = Guid.NewGuid();
        var entry = JournalEntry.Create("LCT-2025-001", "Teste lancamento", new DateTime(2025, 1, 15), JournalEntryType.Manual, periodId, Guid.NewGuid());
        var expected = new JournalEntryResponse(entry.Id, "LCT-2025-001", "Teste lancamento", new DateTime(2025, 1, 15), JournalEntryStatus.Draft, "Draft", JournalEntryType.Manual, "Manual", null, null, null, periodId, "Jan/2025", 0m, 0m, true, [], DateTime.UtcNow, null);
        _repo.GetWithLinesAsync(entry.Id, default).Returns(entry);
        _mapper.Map<JournalEntryResponse>(entry).Returns(expected);
        var result = await CreateHandler().Handle(new GetJournalEntryByIdQuery(entry.Id), default);
        result.Should().NotBeNull();
        result.EntryNumber.Should().Be("LCT-2025-001");
    }

    [Fact]
    public async Task Handle_WithNonExistentEntry_ShouldThrowDomainException()
    {
        _repo.GetWithLinesAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var act = () => CreateHandler().Handle(new GetJournalEntryByIdQuery(Guid.NewGuid()), default);
        await act.Should().ThrowAsync<Exception>();
    }
}

public class GetJournalEntriesByPeriodQueryHandlerTests
{
    private readonly IJournalEntryRepository _repo = Substitute.For<IJournalEntryRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetJournalEntriesByPeriodQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithPeriodId_ShouldReturnEntriesForPeriod()
    {
        var periodId = Guid.NewGuid();
        _repo.GetByPeriodAsync(periodId, default).Returns(new List<JournalEntry>());
        _mapper.Map<IEnumerable<JournalEntrySummaryResponse>>(Arg.Any<object>()).Returns(new List<JournalEntrySummaryResponse>());
        var result = await CreateHandler().Handle(new GetJournalEntriesByPeriodQuery(periodId), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetByPeriodAsync(periodId, default);
    }
}

public class GetPayrollByIdQueryHandlerTests
{
    private readonly IPayrollRepository _repo = Substitute.For<IPayrollRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetPayrollByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingPayroll_ShouldReturnResponse()
    {
        var payroll = Payroll.Create(2025, 1);
        var expected = new PayrollDetailResponse(payroll.Id, 1, 2025, "Jan/2025", "Draft",
            0m, 0m, 0m, null, null, null, DateTime.UtcNow, []);
        _repo.GetByIdWithDetailsAsync(payroll.Id, default).Returns(payroll);
        _mapper.Map<PayrollDetailResponse>(payroll).Returns(expected);
        var result = await CreateHandler().Handle(new GetPayrollByIdQuery(payroll.Id), default);
        result.Should().NotBeNull();
        result!.Year.Should().Be(2025);
    }

    [Fact]
    public async Task Handle_WithNonExistentPayroll_ShouldReturnNull()
    {
        _repo.GetByIdWithDetailsAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetPayrollByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
    }
}

public class GetPayrollHistoryQueryHandlerTests
{
    private readonly IPayrollRepository _repo = Substitute.For<IPayrollRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetPayrollHistoryQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ShouldReturnPagedPayrollHistory()
    {
        _repo.GetHistoryPagedAsync(1, 20, default).Returns((new List<Payroll>().AsReadOnly(), 0));
        _mapper.Map<List<PayrollResponse>>(Arg.Any<object>()).Returns(new List<PayrollResponse>());
        var result = await CreateHandler().Handle(new GetPayrollHistoryQuery(), default);
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }
}

public class GetAllSuppliersQueryHandlerTests
{
    private readonly ISupplierRepository _repo = Substitute.For<ISupplierRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAllSuppliersQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ShouldReturnAllSuppliers()
    {
        _repo.GetActivePagedAsync(1, 20, default).Returns((new List<Supplier>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<SupplierSummaryResponse>>(Arg.Any<object>()).Returns(new List<SupplierSummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAllSuppliersQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetActivePagedAsync(1, 20, default);
    }
}

public class GetSupplierByIdQueryHandlerTests
{
    private readonly ISupplierRepository _repo = Substitute.For<ISupplierRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetSupplierByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingSupplier_ShouldReturnResponse()
    {
        var supplier = Supplier.Create("Fornecedor", "12345678000195", PersonType.Company, "f@email.com");
        var expected = new SupplierResponse(supplier.Id, "Fornecedor", "12345678000195",
            "Company", "f@email.com", null, null, "Active", null, null, null, null, DateTime.UtcNow, null);
        _repo.GetByIdAsync(supplier.Id, default).Returns(supplier);
        _mapper.Map<SupplierResponse>(supplier).Returns(expected);
        var result = await CreateHandler().Handle(new GetSupplierByIdQuery(supplier.Id), default);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Fornecedor");
    }

    [Fact]
    public async Task Handle_WithNonExistentSupplier_ShouldReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetSupplierByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
    }
}

public class GetTaxEntriesQueryHandlerTests
{
    private readonly ITaxEntryRepository _repo = Substitute.For<ITaxEntryRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetTaxEntriesQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithNoFilter_ShouldReturnAllTaxEntries()
    {
        _repo.GetPagedAsync(null, null, null, null, null, null, 1, 20, default).Returns((new List<TaxEntry>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<TaxEntrySummaryResponse>>(Arg.Any<object>()).Returns(new List<TaxEntrySummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetTaxEntriesQuery(null, null, null, null, null, null), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(null, null, null, null, null, null, 1, 20, default);
    }

    [Fact]
    public async Task Handle_WithTaxTypeFilter_ShouldReturnByTaxType()
    {
        _repo.GetPagedAsync(TaxType.INSS, null, null, null, null, null, 1, 20, default).Returns((new List<TaxEntry>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<TaxEntrySummaryResponse>>(Arg.Any<object>()).Returns(new List<TaxEntrySummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetTaxEntriesQuery(TaxType.INSS, null, null, null, null, null), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(TaxType.INSS, null, null, null, null, null, 1, 20, default);
    }
}

public class GetOverdueTaxEntriesQueryHandlerTests
{
    private readonly ITaxEntryRepository _repo = Substitute.For<ITaxEntryRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetOverdueTaxEntriesQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ShouldReturnOverdueTaxEntries()
    {
        _repo.GetOverdueAsync(default).Returns(new List<TaxEntry>().AsReadOnly());
        _mapper.Map<IReadOnlyList<TaxEntrySummaryResponse>>(Arg.Any<object>()).Returns(new List<TaxEntrySummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetOverdueTaxEntriesQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetOverdueAsync(default);
    }
}

public class GetTaxEntryByIdQueryHandlerTests
{
    private readonly ITaxEntryRepository _repo = Substitute.For<ITaxEntryRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();
    private GetTaxEntryByIdQueryHandler CreateHandler() => new(_repo, _mapper, _notif);

    [Fact]
    public async Task Handle_WithExistingEntry_ShouldReturnResponse()
    {
        var entry = TaxEntry.Create(TaxType.INSS, "INSS Jan/2025", 10000m, 0.11m, new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 20));
        var expected = new TaxEntryResponse(entry.Id, "INSS", "INSS Jan/2025",
            10000m, 0.11m, 1100m, "BRL", new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 20),
            "Calculated", null, null, null, null, null, DateTime.UtcNow, null, []);
        _repo.GetWithPaymentsAsync(entry.Id, default).Returns(entry);
        _mapper.Map<TaxEntryResponse>(entry).Returns(expected);
        var result = await CreateHandler().Handle(new GetTaxEntryByIdQuery(entry.Id), default);
        result.Should().NotBeNull();
        result!.Description.Should().Be("INSS Jan/2025");
    }

    [Fact]
    public async Task Handle_WithNonExistentEntry_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithPaymentsAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetTaxEntryByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

public class GetTaxPaymentByIdQueryHandlerTests
{
    private readonly ITaxPaymentRepository _repo = Substitute.For<ITaxPaymentRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();
    private GetTaxPaymentByIdQueryHandler CreateHandler() => new(_repo, _mapper, _notif);

    [Fact]
    public async Task Handle_WithExistingPayment_ShouldReturnResponse()
    {
        var taxEntryId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var payment = TaxPayment.Create(taxEntryId, bankAccountId, 1100m, new DateOnly(2025, 2, 20));
        var expected = new TaxPaymentResponse(payment.Id, taxEntryId, "INSS", bankAccountId,
            "Banco", 1100m, 0m, 0m, 1100m, "BRL", new DateOnly(2025, 2, 20),
            null, null, "Paid", null, DateTime.UtcNow, null);
        _repo.GetWithDetailsAsync(payment.Id, default).Returns(payment);
        _mapper.Map<TaxPaymentResponse>(payment).Returns(expected);
        var result = await CreateHandler().Handle(new GetTaxPaymentByIdQuery(payment.Id), default);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentPayment_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithDetailsAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetTaxPaymentByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

public class GetTaxPaymentsByEntryQueryHandlerTests
{
    private readonly ITaxPaymentRepository _repo = Substitute.For<ITaxPaymentRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetTaxPaymentsByEntryQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ShouldReturnPaymentsForTaxEntry()
    {
        var taxEntryId = Guid.NewGuid();
        _repo.GetByTaxEntryAsync(taxEntryId, default).Returns(new List<TaxPayment>().AsReadOnly());
        _mapper.Map<IReadOnlyList<TaxPaymentResponse>>(Arg.Any<object>()).Returns(new List<TaxPaymentResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetTaxPaymentsByEntryQuery(taxEntryId), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetByTaxEntryAsync(taxEntryId, default);
    }
}



