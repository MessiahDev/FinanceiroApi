using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Status).HasConversion<string>();
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.Property(p => p.IsDeleted);

        builder.OwnsOne(p => p.Period, period =>
        {
            period.Property(d => d.Start).HasColumnName("PeriodStart");
            period.Property(d => d.End).HasColumnName("PeriodEnd");
        });

        builder.OwnsOne(p => p.TotalGross, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalGrossAmount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("TotalGrossCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(p => p.TotalDiscounts, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalDiscountsAmount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("TotalDiscountsCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(p => p.TotalNet, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalNetAmount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("TotalNetCurrency").HasMaxLength(3);
        });

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.PayrollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Items).HasField("_items");
        builder.ToTable("Payrolls");
    }
}

public sealed class PayrollItemConfiguration : IEntityTypeConfiguration<PayrollItem>
{
    public void Configure(EntityTypeBuilder<PayrollItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.IsDeleted);

        builder.OwnsOne(i => i.GrossSalary, money =>
        {
            money.Property(m => m.Amount).HasColumnName("GrossSalaryAmount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("GrossSalaryCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.InssDiscount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("InssDiscountAmount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("InssDiscountCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.IrpfDiscount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("IrpfDiscountAmount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("IrpfDiscountCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.OtherDiscounts, money =>
        {
            money.Property(m => m.Amount).HasColumnName("OtherDiscountsAmount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("OtherDiscountsCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.NetSalary, money =>
        {
            money.Property(m => m.Amount).HasColumnName("NetSalaryAmount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("NetSalaryCurrency").HasMaxLength(3);
        });

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("PayrollItems");
    }
}