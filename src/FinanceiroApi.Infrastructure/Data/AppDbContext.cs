using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Entities.Base;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FinanceiroApi.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    private readonly IMediator? _mediator;

    public AppDbContext(DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider)
    : base(options)
    {
        _mediator = serviceProvider.GetService<IMediator>();
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<AccountPayable> AccountsPayable => Set<AccountPayable>();
    public DbSet<AccountReceivable> AccountsReceivable => Set<AccountReceivable>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Ignore<FinanceiroApi.Domain.Events.Base.DomainEvent>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private readonly HashSet<Guid> _newEntityIds = [];

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _newEntityIds.Clear();
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                _newEntityIds.Add(entry.Entity.Id);
        }

        FixNewEntitiesState();
        SetAuditFields();

        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync(cancellationToken);
        return result;
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreatedAt(DateTime.UtcNow);

            entry.Entity.SetUpdatedAt(DateTime.UtcNow);
        }
    }

    private void FixNewEntitiesState()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().ToList())
        {
            if (entry.State != EntityState.Modified) continue;

            var originalCreatedAt = entry.OriginalValues[nameof(BaseEntity.CreatedAt)];
            if (originalCreatedAt is DateTime dt && dt == entry.Entity.CreatedAt
                && entry.Entity.UpdatedAt == null)
            {
                entry.State = EntityState.Added;
            }
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (_mediator is null) return;

        var entities = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var events = entities.SelectMany(e => e.DomainEvents).ToList();
        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in events)
            await _mediator.Publish(domainEvent, cancellationToken);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) return;
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
                      .EnableSensitiveDataLogging();
    }
}
