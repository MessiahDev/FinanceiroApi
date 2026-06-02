using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Position).HasMaxLength(100);
        builder.Property(e => e.ContractType).HasConversion<string>();
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.IsDeleted);

        builder.OwnsOne(e => e.Cpf, cpf =>
        {
            cpf.Property(c => c.Value).HasColumnName("Cpf").IsRequired().HasMaxLength(14);
            cpf.HasIndex(c => c.Value).IsUnique();
        });

        builder.OwnsOne(e => e.Email, email =>
        {
            email.Property(em => em.Value).HasColumnName("Email").IsRequired().HasMaxLength(200);
            email.HasIndex(em => em.Value).IsUnique();
        });

        builder.OwnsOne(e => e.BaseSalary, money =>
        {
            money.Property(m => m.Amount).HasColumnName("BaseSalaryAmount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("BaseSalaryCurrency").HasMaxLength(3);
        });

        builder.Ignore(e => e.FullName);
        builder.ToTable("Employees");
    }
}