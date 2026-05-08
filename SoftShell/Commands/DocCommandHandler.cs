using SoftShell.Core;
using SoftShell.Core.data;
using Spectre.Console;

namespace SoftShell.Commands;

public class DocCommandHandler(IDocProvider docProvider) : ICommandDefinedHandler
{
    public Task HandleAsync(string[] args)
    {
        var command = args[0];
        var info = docProvider.GetDescription(command) ?? GetCommandHelp(command);

        if (string.IsNullOrEmpty(info))
        {
            AnsiConsole.MarkupLine($"[red]No documentation found for '{Markup.Escape(command)}'[/]");
            return Task.CompletedTask;
        }
        
        ShowInfo(command, info);
        return Task.CompletedTask;
    }

    public static Command Definition { get; } = new Command
    {
        Name = "doc",
        Description = "Provides documentation for a given topic or command",
        Aliases = ["d"],
        Flags = []
    };

    private void ShowInfo(string command, string description)
    {
        AnsiConsole.MarkupLine($"[red]{command}[/]");
        AnsiConsole.MarkupLine($"[red]{description}[/]");
    }

    private string? GetCommandHelp(string command)
    {
        var helpCommand = $"{command} --help";
        var (output, _) = PsCommandExecutor.ExecutePowerShellWithReturn(helpCommand);
        return output;
    }
}