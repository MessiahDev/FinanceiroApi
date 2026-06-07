using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class AccountReceivableConfiguration : IEntityTypeConfiguration<AccountReceivable>
{
    public void Configure(EntityTypeBuilder<AccountReceivable> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.InvoiceNumber).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.OwnsOne(e => e.TotalAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("TotalAmount").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("TotalCurrency").HasMaxLength(3);
        });
        builder.OwnsOne(e => e.ReceivedAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("ReceivedAmount").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("ReceivedCurrency").HasMaxLength(3);
        });

        builder.Ignore(e => e.RemainingAmount);
        builder.ToTable("AccountsReceivable");
    }
}
