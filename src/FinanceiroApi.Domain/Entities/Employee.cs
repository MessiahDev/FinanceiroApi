using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class Employee : AggregateRoot
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string FullName => $"{FirstName} {LastName}";
    public Cpf Cpf { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public Money BaseSalary { get; private set; } = default!;
    public ContractType ContractType { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public DateOnly HireDate { get; private set; }
    public DateOnly? TerminationDate { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Position? Position { get; private set; }

    public Department? Department { get; private set; }

    protected Employee() { }

    public static Employee Create(
        string firstName,
        string lastName,
        string cpf,
        string email,
        decimal baseSalary,
        ContractType contractType,
        Guid departmentId,
        Position? position = null,
        DateOnly? hireDate = null)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new DomainException("Last name is required.");

        var employee = new Employee
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Cpf = new Cpf(cpf),
            Email = new Email(email),
            BaseSalary = new Money(baseSalary),
            ContractType = contractType,
            Status = EmployeeStatus.Active,
            DepartmentId = departmentId,
            Position = position,
            HireDate = hireDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
        };

        employee.AddDomainEvent(new EmployeeCreatedEvent(employee.Id, employee.FullName, employee.Email));
        return employee;
    }

    public void UpdatePersonalInfo(string firstName, string lastName, string email, Position? position)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new DomainException("Last name is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = new Email(email);
        Position = position;
        SetUpdatedAt();
    }

    public void UpdateSalary(decimal newSalary)
    {
        var oldSalary = BaseSalary;
        BaseSalary = new Money(newSalary);
        SetUpdatedAt();

        AddDomainEvent(new EmployeeSalaryUpdatedEvent(Id, oldSalary, BaseSalary));
    }

    public void TransferToDepartment(Guid newDepartmentId)
    {
        if (newDepartmentId == DepartmentId)
            throw new DomainException("Employee is already in this department.");

        DepartmentId = newDepartmentId;
        SetUpdatedAt();
    }

    public void Terminate(DateOnly terminationDate)
    {
        if (Status == EmployeeStatus.Terminated)
            throw new DomainException("Employee is already terminated.");

        Status = EmployeeStatus.Terminated;
        TerminationDate = terminationDate;
        SetUpdatedAt();

        AddDomainEvent(new EmployeeTerminatedEvent(Id, FullName, terminationDate));
    }

    public void PlaceOnLeave()
    {
        EnsureActive();
        Status = EmployeeStatus.OnLeave;
        SetUpdatedAt();
    }

    public void ReturnFromLeave()
    {
        if (Status != EmployeeStatus.OnLeave)
            throw new DomainException("Employee is not on leave.");

        Status = EmployeeStatus.Active;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        EnsureActive();
        Status = EmployeeStatus.Inactive;
        SetUpdatedAt();
    }

    public void Activate()
    {
        if (Status == EmployeeStatus.Terminated)
            throw new DomainException("Cannot reactivate a terminated employee.");

        Status = EmployeeStatus.Active;
        SetUpdatedAt();
    }

    private void EnsureActive()
    {
        if (Status != EmployeeStatus.Active)
            throw new DomainException($"Employee is not active. Current status: {Status}.");
    }
}