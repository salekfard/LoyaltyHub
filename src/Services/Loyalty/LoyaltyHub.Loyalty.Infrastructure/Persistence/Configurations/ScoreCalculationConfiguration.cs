using LoyaltyHub.Loyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltyHub.Loyalty.Infrastructure.Persistence.Configurations;

public sealed class ScoreCalculationConfiguration : IEntityTypeConfiguration<ScoreCalculation>
{
    public void Configure(EntityTypeBuilder<ScoreCalculation> builder)
    {
        builder.ToTable("ScoreCalculations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PurchaseAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CustomerType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.BaseScore).HasPrecision(18, 4);
        builder.Property(x => x.FrequencyBonus).HasPrecision(18, 4);
        builder.Property(x => x.HighPurchaseBonus).HasPrecision(18, 4);
        builder.Property(x => x.TotalBonus).HasPrecision(18, 4);
        builder.Property(x => x.FinalScore).HasPrecision(18, 4);

        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasMany(x => x.RecentPurchases)
            .WithOne(x => x.ScoreCalculation)
            .HasForeignKey(x => x.ScoreCalculationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.RecentPurchases)
            .HasField("_recentPurchases")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
