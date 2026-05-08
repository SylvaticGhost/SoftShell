using SoftShell.Core;
using SoftShell.Infra;
using Spectre.Console;

namespace SoftShell.Commands;

public class SetCredentialCommandHandler(CredentialProvider credentialProvider) : ICommandDefinedHandler
{
    public async Task HandleAsync(string[] args)
    {
        string? name = null;
        string? value = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-n" || args[i] == "--name")
            {
                if (i + 1 < args.Length) name = args[++i];
            }
            else if (args[i] == "-v" || args[i] == "--value")
            {
                if (i + 1 < args.Length) value = args[++i];
            }
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
        {
            AnsiConsole.MarkupLine($"[{ColorPalette.Primary}]Error:[/] Both [rgb(255,184,224)]--name[/] and [rgb(255,184,224)]--value[/] are required.");
            return;
        }

        await credentialProvider.SetValueAsync(name, value);
        AnsiConsole.MarkupLine($"[{ColorPalette.Tertiary}]Credential [[{Markup.Escape(name)}]] set successfully.[/]");
    }

    public static Command Definition { get; } = new()
    {
        Name = "credential set",
        Aliases = ["cs"],
        Description = "Sets a credential for the given credentials.",
        Flags =
        [
            new Flag
            {
                Name = "name",
                ShortName = "n",
                Description = "The name of the credentials."
            },
            new Flag()
            {
                Name = "value",
                ShortName = "v",
                Description = "The value of the credentials."
            }
        ]
    };
}