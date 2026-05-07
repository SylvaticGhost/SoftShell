namespace SoftShell.Core;

public record Command
{
    public const string Prefix = "/";
    
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string[] Aliases { get; init; } = [];
    public Flag[] Flags { get; init; } = [];
}