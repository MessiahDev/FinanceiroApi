using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanceiroApi.Domain.Entities;

namespace FinanceiroApi.Infrastructure.Configurations;

public class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.ToTable("AccountingPeriods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.OwnsOne(x => x.Period, period =>
        {
            period.Property(p => p.Start)
                .HasColumnName("PeriodStart")
                .IsRequired();

            period.Property(p => p.End)
                .HasColumnName("PeriodEnd")
                .IsRequired();
        });

        builder.HasIndex(x => new { x.Year, x.Month })
            .IsUnique()
            .HasDatabaseName("IX_AccountingPeriods_YearMonth");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_AccountingPeriods_Status");

        builder.Ignore("_domainEvents");
    }
}
