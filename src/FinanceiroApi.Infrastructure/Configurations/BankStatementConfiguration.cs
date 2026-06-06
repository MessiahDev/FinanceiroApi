using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.FileName)
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.OwnsOne(e => e.OpeningBalance, m =>
        {
            m.Property(x => x.Amount).HasColumnName("OpeningBalance").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("OpeningCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(e => e.ClosingBalance, m =>
        {
            m.Property(x => x.Amount).HasColumnName("ClosingBalance").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("ClosingCurrency").HasMaxLength(3);
        });

        builder.Ignore(e => e.TotalEntries);
        builder.Ignore(e => e.TotalCredits);
        builder.Ignore(e => e.TotalDebits);

        builder.HasOne(e => e.BankAccount)
            .WithMany()
            .HasForeignKey(e => e.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Entries)
            .WithOne(e => e.BankStatement)
            .HasForeignKey(e => e.BankStatementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("BankStatements");
    }
}

public sealed class BankStatementEntryConfiguration : IEntityTypeConfiguration<BankStatementEntry>
{
    public void Configure(EntityTypeBuilder<BankStatementEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.EntryType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.DocumentNumber)
            .HasMaxLength(100);

        builder.OwnsOne(e => e.Amount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("Amount").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.ToTable("BankStatementEntries");
    }
}
