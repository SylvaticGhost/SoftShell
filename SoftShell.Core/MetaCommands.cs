namespace SoftShell.Core;

public static class MetaCommands
{
    private static readonly string[] ExitKeywords = ["exit", "quit"];
    public static bool IsExitRequest(string prompt) => InKeywords(prompt, ExitKeywords);

    private static readonly string[] ClearKeywords = ["clear", "cls"];
    public static bool IsClearRequest(string prompt) => InKeywords(prompt, ClearKeywords);

    private static bool InKeywords(string prompt, string[] keywords) =>
        keywords.Contains(prompt, StringComparer.OrdinalIgnoreCase) || prompt.StartsWith("/") &&
        keywords.Contains(prompt[1..], StringComparer.OrdinalIgnoreCase);

    public static readonly Command[] Definitions =
    [
        new()
        {
            Name = "exit",
            Description = "Exits the terminal",
            Aliases = ["quit"],
        },
        new()
        {
            Name = "clear",
            Description = "Clears the terminal",
            Aliases = ["cls"],
        }
    ];
}