using SoftShell.Core;
using SoftShell.LLM;
using Spectre.Console;

namespace SoftShell.Commands;

public class AskCommandHandler(ILlmService llmService) : ICommandDefinedHandler
{
    public static Command Definition { get; } = new Command
    {
        Name = "ask",
        Description = "Suggest powershell command based on provided context",
        Aliases = ["a"],
        Flags = []
    };

    public async Task HandleAsync(string[] args)
    {
        var arguments = string.Join(' ', args);
        if (string.IsNullOrWhiteSpace(arguments))
        {
            AnsiConsole.MarkupLine("[rgb(190,89,133)]Error:[/] Define the outcome.");
            return;
        }

        var response = await llmService.GenerateCommandAsync(arguments);
        
        if (response != null && !string.IsNullOrEmpty(response.Command))
        {
            ShowCommandPanel(response.Command, response.Explanation);
        }
    }

    private void ShowCommandPanel(string command, string description)
    {
        PsCommandExecutor.HandleGeneratedCommand(command, "Suggested Command", description);
    }
}