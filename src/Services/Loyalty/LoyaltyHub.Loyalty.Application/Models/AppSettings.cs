namespace LoyaltyHub.Loyalty.Application.Models;

public record AppSettings
{
    public string? Version { get; set; }

    public string? Environment { get; set; }

    public bool IsSwaggerActivated { get; set; }

    public bool IsSeedDataActivated { get; set; }

    public bool IsGrpcReflectionActivated { get; set; }
}
