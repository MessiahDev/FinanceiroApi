using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FinanceiroApi.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    private static Money ToMoney(decimal v) => new Money(v);

    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Status).HasConversion<string>();

        builder.Property(e => e.TotalPlanned)
               .HasColumnName("TotalPlanned")
               .HasColumnType("numeric(18,2)")
               .HasConversion(
                   m => m.Amount,
                   v => ToMoney(v));

        builder.Property(e => e.TotalRealized)
               .HasColumnName("TotalRealized")
               .HasColumnType("numeric(18,2)")
               .HasConversion(
                   m => m.Amount,
                   v => ToMoney(v));

        builder.Ignore(e => e.Variance);

        builder.HasMany(b => b.Items)
               .WithOne()
               .HasForeignKey(i => i.BudgetId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("Budgets");
    }
}

public sealed class BudgetItemConfiguration : IEntityTypeConfiguration<BudgetItem>
{
    private static Money ToMoney(decimal v) => new Money(v);

    public void Configure(EntityTypeBuilder<BudgetItem> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.BudgetId).IsRequired();
        builder.Property(e => e.CostCenterId).IsRequired();
        builder.Property(e => e.Category).IsRequired().HasMaxLength(200);

        builder.Property(e => e.PlannedAmount)
               .HasColumnName("PlannedAmount")
               .HasColumnType("numeric(18,2)")
               .HasConversion(
                   m => m.Amount,
                   v => ToMoney(v));

        builder.Property(e => e.RealizedAmount)
               .HasColumnName("RealizedAmount")
               .HasColumnType("numeric(18,2)")
               .HasConversion(
                   m => m.Amount,
                   v => ToMoney(v));

        builder.Ignore(e => e.Variance);
        builder.Ignore(e => e.IsOverBudget);

        builder.ToTable("BudgetItems");
    }
}
