namespace SoftShell.Core;

public record Flag
{
    public required string Name { get; init; }
    public string? ShortName { get; init; }
    public required string Description { get; init; }
}