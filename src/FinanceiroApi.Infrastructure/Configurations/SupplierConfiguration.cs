using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.TaxId).IsRequired().HasMaxLength(20);
        builder.Property(e => e.PersonType).HasConversion<string>();
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.ContactName).HasMaxLength(200);
        builder.Property(e => e.BankName).HasMaxLength(200);
        builder.Property(e => e.BankAgency).HasMaxLength(20);
        builder.Property(e => e.BankAccount).HasMaxLength(20);
        builder.Property(e => e.PixKey).HasMaxLength(150);

        builder.OwnsOne(e => e.Email, email => {
            email.Property(em => em.Value).HasColumnName("Email").IsRequired().HasMaxLength(200);
        });

        builder.ToTable("Suppliers");
    }
}
