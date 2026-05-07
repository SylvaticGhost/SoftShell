using SoftShell.Core;
using Spectre.Console;

namespace SoftShell.Commands;

internal sealed class HelpCommand(ICommandRegistry commandRegistry) : ICommandDefinedHandler
{
    public static Command Definition { get; } = new()
    {
        Name = "help",
        Description = "Displays help information"
    };

    public Task HandleAsync(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine($"[{ColorPalette.Primary}]Available commands:[/]");
            foreach (var def in commandRegistry.GetAll())
                AnsiConsole.MarkupLine(
                    $"  [{ColorPalette.Secondary}]{Markup.Escape(def.Name)}[/] - {Markup.Escape(def.Description)}");
            return Task.CompletedTask;
        }

        var command = args[0];

        var commandDefinition = commandRegistry.GetDefinition(command);
        if (commandDefinition is null)
        {
            CommandNotFoundOutput(command);
            return Task.CompletedTask;
        }

        OutputCommand(commandDefinition);
        return Task.CompletedTask;
    }

    private static void CommandNotFoundOutput(string requestedCommand)
    {
        AnsiConsole.MarkupLine($"[{ColorPalette.Tertiary}]Command '{Markup.Escape(requestedCommand)}' not found.[/]");
    }

    private static void OutputCommand(Command command)
    {
        AnsiConsole.MarkupLine(
            $"[{ColorPalette.Primary}]{Markup.Escape(command.Name)}[/] - {Markup.Escape(command.Description)}");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(
            $"Aliases: {string.Join(", ", command.Aliases.Select(a => $"[{ColorPalette.Secondary}]{Markup.Escape(a)}[/]"))}");
        AnsiConsole.MarkupLine(
            $"Flags: {string.Join(", ", command.Flags.Select(f => $"[{ColorPalette.Secondary}]{Markup.Escape(f.Name)}[/]"))}");
        AnsiConsole.MarkupLine($"Description: {Markup.Escape(command.Description)}");
        AnsiConsole.WriteLine();
    }
}