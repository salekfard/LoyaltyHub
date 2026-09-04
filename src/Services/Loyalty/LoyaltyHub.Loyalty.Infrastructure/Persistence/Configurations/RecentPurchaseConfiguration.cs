using LoyaltyHub.Loyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltyHub.Loyalty.Infrastructure.Persistence.Configurations;

public sealed class RecentPurchaseConfiguration : IEntityTypeConfiguration<RecentPurchase>
{
    public void Configure(EntityTypeBuilder<RecentPurchase> builder)
    {
        builder.ToTable("RecentPurchases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Sequence)
            .IsRequired();

        builder.HasIndex(x => new { x.ScoreCalculationId, x.Sequence })
            .IsUnique();
    }
}
