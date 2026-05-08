using System.Text;
using SoftShell.Core;
using SoftShell.Core.data;
using SoftShell.LLM;
using Spectre.Console;

namespace SoftShell.Commands;

public class TranslateCommandHandler(ICommandTranslator translator, IDocProvider docProvider, ILlmService llmService)
    : ICommandDefinedHandler
{
    public static Command Definition { get; } = new()
    {
        Name = "translate",
        Aliases = ["t"],
        Description = "Translates a bash-like command to its PowerShell equivalent.",
    };
    
    public async Task HandleAsync(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine(
                "[rgb(190,89,133)]Error:[/] No command provided. Usage: [rgb(255,184,224)]translate[/]");
            return;
        }

        string command = args[0];
        if (string.IsNullOrWhiteSpace(command))
        {
            AnsiConsole.MarkupLine("[rgb(190,89,133)]Error:[/] Specify Bash-command.");
            return;
        }

        string? description, psCommand;
        if (translator.TryTranslate(command, out var psc) && !string.IsNullOrEmpty(psc))
        {
            description = docProvider.GetDescription(psc);
            psCommand = psc;
        }
        else
        {
            AnsiConsole.MarkupLine("[rgb(255,184,224)]Requesting AI...[/]");
            var result = await llmService.TranslateBashAsync(command);
            psCommand = result?.Command;
            description = result?.Explanation;
        }
        
        if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(psCommand)) 
            AnsiConsole.MarkupLine($"[red]No translation found for '{Markup.Escape(command)}'[/]");
        else
            ShowActionMenu(psCommand, description);

    }

    private static void ShowActionMenu(string command, string description)
    {
        var sb = new StringBuilder("This is the PowerShell equivalent of your Bash command.");
        if (!string.IsNullOrWhiteSpace(description))
            sb.AppendLine(description);

        PsCommandExecutor.HandleGeneratedCommand(command, "Translated Command", sb.ToString());
    }
}