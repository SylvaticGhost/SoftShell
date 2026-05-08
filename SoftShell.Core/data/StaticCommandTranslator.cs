namespace SoftShell.Core.data;

public class StaticCommandTranslator : ICommandTranslator
{
    private readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ls", "Get-ChildItem" },
        { "pwd", "Get-Location" },
        { "rm", "Remove-Item" },
        { "cat", "Get-Content" },
        { "grep", "Select-String" },
        { "clear", "Clear-Host" },
        { "cp", "Copy-Item" },
        { "mv", "Move-Item" },
        { "mkdir", "New-Item -ItemType Directory" },
        { "touch", "New-Item -ItemType File" },
        { "kill", "Stop-Process" },
        { "man", "Get-Help" }
    };

    public bool TryTranslate(string bashCommand, out string? psCommand)
    {
        var baseCommand = bashCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        
        if (baseCommand != null && _map.TryGetValue(baseCommand, out var mappedCmd))
        {
            psCommand = mappedCmd;
            return true;
        }

        psCommand = null;
        return false;
    }
}