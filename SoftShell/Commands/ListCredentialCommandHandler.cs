using SoftShell.Core;
using SoftShell.Infra;
using Spectre.Console;

namespace SoftShell.Commands;

public class ListCredentialCommandHandler(CredentialProvider credentialProvider) : ICommandDefinedHandler
{
    public async Task HandleAsync(string[] args)
    {
        var keys = await credentialProvider.ListAllAsync();

        if (keys.Length == 0)
        {
            AnsiConsole.MarkupLine($"[{ColorPalette.Tertiary}]No credentials found.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Square)
            .BorderColor(ColorPalette.Primary.ToColor())
            .AddColumn(new TableColumn($"[{ColorPalette.Primary}]Key[/]").Centered())
            .Title($"[{ColorPalette.Primary}]Stored Credentials[/]");

        foreach (var key in keys)
        {
            table.AddRow($"[{ColorPalette.Secondary}]{Markup.Escape(key)}[/]");
        }

        AnsiConsole.Write(table);
    }

    public static Command Definition { get; } = new()
    {
        Name = "credential list",
        Aliases = ["cl"],
        Description = "Lists all stored credentials.",
    };
}