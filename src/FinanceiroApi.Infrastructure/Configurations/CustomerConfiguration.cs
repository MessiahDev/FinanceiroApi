using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.TaxId).IsRequired().HasMaxLength(20);
        builder.Property(e => e.PersonType).HasConversion<string>();
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.ContactName).HasMaxLength(200);

        builder.OwnsOne(e => e.Email, email => {
            email.Property(em => em.Value).HasColumnName("Email").IsRequired().HasMaxLength(200);
        });
        builder.OwnsOne(e => e.CreditLimit, m => {
            m.Property(x => x.Amount).HasColumnName("CreditLimit").HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency).HasColumnName("CreditCurrency").HasMaxLength(3);
        });

        builder.ToTable("Customers");
    }
}
