using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class TaxPaymentConfiguration : IEntityTypeConfiguration<TaxPayment>
{
    public void Configure(EntityTypeBuilder<TaxPayment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DarfNumber)
            .HasMaxLength(100);

        builder.Property(e => e.ReceiptCode)
            .HasMaxLength(200);

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

        builder.OwnsOne(e => e.Fine, m =>
        {
            m.Property(x => x.Amount).HasColumnName("Fine").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("FineCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(e => e.Interest, m =>
        {
            m.Property(x => x.Amount).HasColumnName("Interest").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("InterestCurrency").HasMaxLength(3);
        });

        builder.Ignore(e => e.TotalPaid);

        builder.HasOne(e => e.TaxEntry)
            .WithMany(t => t.Payments)
            .HasForeignKey(e => e.TaxEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BankAccount)
            .WithMany()
            .HasForeignKey(e => e.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("TaxPayments");
    }
}
