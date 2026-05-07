namespace SoftShell.Core.data;

public class StaticDocProvider : IDocProvider
{
   private readonly Dictionary<string, string> _docs = new(StringComparer.OrdinalIgnoreCase)
   {
       { "Get-ChildItem", "Gets the items and child items in one or more specified locations." },
       { "Get-Location", "Gets information about the current working directory." },
       { "Remove-Item", "Deletes the specified items \u0028files, folders\u0029." },
       { "Get-Content", "Gets the content of an item at the specified path \u0028file reading\u0029." },
       { "Select-String", "Searches text in lines and files \u0028similar to grep\u0029." },
       { "Clear-Host", "Clears the terminal screen." },
       { "Copy-Item", "Copies an item from one location to another." },
       { "Move-Item", "Moves an item from one location to another." },
       { "New-Item", "Creates a new item \u0028file or folder\u0029." },
       { "Stop-Process", "Stops one or more running processes." },
       { "Get-Help", "Displays help information about PowerShell cmdlets." }
   };

    public string? GetDescription(string psCommand)
    {
        if (string.IsNullOrWhiteSpace(psCommand)) return null;

        // Відрізаємо можливі аргументи (наприклад "New-Item -ItemType Directory" -> "New-Item")
        var baseCommand = psCommand.Split(' ')[0];

        return _docs.TryGetValue(baseCommand, out var doc) 
            ? doc 
            : null;
    }   
}