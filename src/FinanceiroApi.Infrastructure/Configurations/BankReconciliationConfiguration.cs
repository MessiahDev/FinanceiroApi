using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class BankReconciliationConfiguration : IEntityTypeConfiguration<BankReconciliation>
{
    public void Configure(EntityTypeBuilder<BankReconciliation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.OwnsOne(e => e.StatementOpeningBalance, m =>
        {
            m.Property(x => x.Amount).HasColumnName("StatementOpeningBalance").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("StatementOpeningCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(e => e.StatementClosingBalance, m =>
        {
            m.Property(x => x.Amount).HasColumnName("StatementClosingBalance").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("StatementClosingCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(e => e.SystemBalance, m =>
        {
            m.Property(x => x.Amount).HasColumnName("SystemBalance").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("SystemBalanceCurrency").HasMaxLength(3);
        });

        builder.Ignore(e => e.Difference);
        builder.Ignore(e => e.IsBalanced);
        builder.Ignore(e => e.TotalItems);
        builder.Ignore(e => e.MatchedItems);
        builder.Ignore(e => e.UnmatchedItems);

        builder.HasOne(e => e.BankAccount)
            .WithMany()
            .HasForeignKey(e => e.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BankStatement)
            .WithMany()
            .HasForeignKey(e => e.BankStatementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.BankReconciliation)
            .HasForeignKey(i => i.BankReconciliationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("BankReconciliations");
    }
}

public sealed class BankReconciliationItemConfiguration : IEntityTypeConfiguration<BankReconciliationItem>
{
    public void Configure(EntityTypeBuilder<BankReconciliationItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.OwnsOne(e => e.Amount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("Amount").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.HasOne(e => e.BankStatementEntry)
            .WithMany()
            .HasForeignKey(e => e.BankStatementEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Transaction)
            .WithMany()
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("BankReconciliationItems");
    }
}
