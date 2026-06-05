using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanceiroApi.Domain.Entities;

namespace FinanceiroApi.Infrastructure.Configurations;

public class ChartOfAccountConfiguration : IEntityTypeConfiguration<ChartOfAccount>
{
    public void Configure(EntityTypeBuilder<ChartOfAccount> builder)
    {
        builder.ToTable("ChartOfAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.AccountType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.AccountNature)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.AcceptsEntries)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_ChartOfAccounts_Code");

        builder.HasIndex(x => x.AccountType)
            .HasDatabaseName("IX_ChartOfAccounts_AccountType");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_ChartOfAccounts_IsActive");

        builder.HasOne(x => x.ParentAccount)
            .WithMany(x => x.ChildAccounts)
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Ignore("_domainEvents");
    }
}
