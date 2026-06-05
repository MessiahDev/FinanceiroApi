using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Status).HasConversion<string>();

        builder.OwnsOne(e => e.AnnualBudget, m => {
            m.Property(x => x.Amount).HasColumnName("AnnualBudget").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("BudgetCurrency").HasMaxLength(3);
        });

        builder.HasOne(e => e.Parent).WithMany(e => e.Children).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable("CostCenters");
    }
}
