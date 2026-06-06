using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class TaxEntryConfiguration : IEntityTypeConfiguration<TaxEntry>
{
    public void Configure(EntityTypeBuilder<TaxEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TaxType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Rate)
            .HasColumnType("numeric(8,4)")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ReferenceDocument)
            .HasMaxLength(200);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.OwnsOne(e => e.BaseAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("BaseAmount").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("BaseCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(e => e.TaxAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("TaxAmount").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("TaxCurrency").HasMaxLength(3);
        });

        builder.HasMany(e => e.Payments)
            .WithOne(p => p.TaxEntry)
            .HasForeignKey(p => p.TaxEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CostCenter)
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("TaxEntries");
    }
}
