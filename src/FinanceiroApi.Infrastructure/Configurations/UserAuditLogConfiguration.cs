using FinanceiroApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceiroApi.Infrastructure.Configurations;

public sealed class UserAuditLogConfiguration : IEntityTypeConfiguration<UserAuditLog>
{
    public void Configure(EntityTypeBuilder<UserAuditLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Action).IsRequired().HasMaxLength(50);
        builder.Property(l => l.OldValue).HasMaxLength(200);
        builder.Property(l => l.NewValue).HasMaxLength(200);

        builder.HasOne(l => l.TargetUser)
            .WithMany()
            .HasForeignKey(l => l.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.ChangedByUser)
            .WithMany()
            .HasForeignKey(l => l.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("UserAuditLogs");
    }
}
