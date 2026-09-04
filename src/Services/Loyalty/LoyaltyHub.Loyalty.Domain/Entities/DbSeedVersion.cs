namespace LoyaltyHub.Loyalty.Domain.Entities;

public class DbSeedVersion : EntityBase
{
    public uint Version { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    private DbSeedVersion()
    {
    }

    public static DbSeedVersion Create(uint version, string description, DateTime createdAtUtc)
    {
        if (version == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Seed version must be greater than zero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new DbSeedVersion
        {
            Version = version,
            Description = description,
            CreatedAtUtc = DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc)
        };
    }
}
