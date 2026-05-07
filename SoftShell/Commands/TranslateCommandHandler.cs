using SoftShell.Core;
using SoftShell.Core.data;
using Spectre.Console;

namespace SoftShell.Commands;

public class TranslateCommandHandler(ICommandTranslator translator, IDocProvider docProvider) : ICommandDefinedHandler
{
    public Task HandleAsync(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[rgb(190,89,133)]Error:[/] No command provided. Usage: [rgb(255,184,224)]translate[/]");
            return Task.CompletedTask;
        }
        
        string arguments = args[0];
        if (string.IsNullOrWhiteSpace(arguments))
        {
            AnsiConsole.MarkupLine("[rgb(190,89,133)]Error:[/] Specify Bash-command.");
            return Task.CompletedTask;
        }
        
        if (translator.TryTranslate(arguments, out var psCommand))
        {
            var description = docProvider.GetDescription(psCommand);
            
            // Відмальовуємо UI
            ShowCommandPanel(psCommand, description);
            
            // Тут буде виклик меню [E/C/R]
            ShowActionMenu(psCommand);
        }
        else
        {
            // Крок 2: LLM Fallback (якщо команди немає у словнику)
            AnsiConsole.MarkupLine($"[rgb(255,184,224)]Команду не знайдено в локальній базі. Звернення до AI...[/]");
            // await LlmService.TranslateAsync(arguments);
        }

        return Task.CompletedTask;
    }
    
    private void ShowCommandPanel(string command, string description)
    {
        var panel = new Panel(
            new Markup($"[rgb(236,127,169)]{command}[/]\n\n[rgb(255,184,224)]{description}[/]")
        )
        {
            Border = BoxBorder.Square, // Строгі квадратні форми (Anime/HUD стиль)
            Header = new PanelHeader("[rgb(190,89,133)] Translation  [/]", Justify.Left)
        };

        AnsiConsole.WriteLine();
        AnsiConsole.Write(panel);
    }

    private void ShowActionMenu(string command)
    {
        AnsiConsole.MarkupLine("[rgb(255,237,250)]Press Enter to resume...[/]");
    }

    public static Command Definition { get; } = new()
    {
        Name = "translate",
        Aliases = ["t"],
        Description = "Translates a bash-like command to its PowerShell equivalent.",
    };
}