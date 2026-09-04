using LoyaltyHub.Loyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltyHub.Loyalty.Infrastructure.Persistence.Configurations;

public sealed class DbSeedVersionConfiguration : IEntityTypeConfiguration<DbSeedVersion>
{
    public void Configure(EntityTypeBuilder<DbSeedVersion> builder)
    {
        builder.ToTable("DbSeedVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .IsRequired();

        builder.HasIndex(x => x.Version)
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
