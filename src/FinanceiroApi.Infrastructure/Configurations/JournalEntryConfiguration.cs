using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinanceiroApi.Domain.Entities;

namespace FinanceiroApi.Infrastructure.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntryNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.EntryDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.EntryType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ReferenceDocument)
            .HasMaxLength(100);

        builder.Property(x => x.ReferenceDocumentType)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.EntryNumber)
            .IsUnique()
            .HasDatabaseName("IX_JournalEntries_EntryNumber");

        builder.HasIndex(x => x.AccountingPeriodId)
            .HasDatabaseName("IX_JournalEntries_AccountingPeriodId");

        builder.HasIndex(x => x.EntryDate)
            .HasDatabaseName("IX_JournalEntries_EntryDate");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_JournalEntries_Status");

        builder.HasIndex(x => new { x.ReferenceDocumentType, x.ReferenceDocumentId })
            .HasDatabaseName("IX_JournalEntries_ReferenceDocument");

        builder.HasOne(x => x.AccountingPeriod)
            .WithMany(x => x.JournalEntries)
            .HasForeignKey(x => x.AccountingPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.JournalEntry)
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore("_domainEvents");
    }
}
