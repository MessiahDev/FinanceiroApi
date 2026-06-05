using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.BankName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.BankCode).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Agency).IsRequired().HasMaxLength(20);
        builder.Property(e => e.AccountNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.AccountType).HasConversion<string>();
        builder.Property(e => e.PixKey).HasMaxLength(150);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.OwnsOne(e => e.Balance, m => {
            m.Property(x => x.Amount).HasColumnName("Balance").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.ToTable("BankAccounts");
    }
}
