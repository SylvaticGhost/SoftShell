using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SoftShell.Commands;
using SoftShell.Core;
using SoftShell.Core.data;
using SoftShell.Infra;
using SoftShell.LLM;
using Spectre.Console;
using TextCopy;

namespace SoftShell;

class Program
{
    private static string _currentDirectory = Directory.GetCurrentDirectory();
    
    static void Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IDocProvider, StaticDocProvider>();
        services.AddSingleton<ICommandTranslator, StaticCommandTranslator>();
        services.AddSingleton<ILlmService, GroqLlmService>();
        services.AddSingleton<CredentialProvider>();
        services.AddTransient<LivePrompt>();
        services.AddCommandHandlers(r =>
        {
            r.AddHandling<HelpCommand>();
            r.AddHandling<TranslateCommandHandler>();
            r.AddHandling<AskCommandHandler>();
            r.AddHandling<ListCredentialCommandHandler>();
            r.AddHandling<SetCredentialCommandHandler>();
            r.AddHandling<AskVoiceCommandHandler>();
            r.AddHandling<DocCommandHandler>();
        });
        
        var serviceProvider = services.BuildServiceProvider();
        var livePrompt = serviceProvider.GetRequiredService<LivePrompt>();
        
        Console.Title = "SoftShell - Terminal";
        AnsiConsole.Clear();
        
        var logo = new FigletText("SoftShell")
            .Color(ColorPalette.Primary)
            .Centered();
        AnsiConsole.Write(logo);
        
        AnsiConsole.Write(new Rule($"[{ColorPalette.Primary}]v1.0.0[/]").RuleStyle(ColorPalette.Primary.ToStyle()).LeftJustified());
        AnsiConsole.WriteLine();

        Console.TreatControlCAsInput = true;

        // REPL Loop
        while (true)
        {
            string? input = livePrompt.ReadLine(_currentDirectory);

            if (input == null) break;
            if (string.IsNullOrWhiteSpace(input)) continue;
            if (MetaCommands.IsExitRequest(input)) break;
            if (MetaCommands.IsClearRequest(input))
            {
                AnsiConsole.Clear();
                continue;
            }

            ProcessInput(input, serviceProvider);
        }
    }

    private static void ProcessInput(string input, IServiceProvider serviceProvider)
    {
        if (input.StartsWith("/"))
        {
            var payload = input[1..];
            var tokens = payload.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var command = tokens.Length > 0 ? tokens[0] : string.Empty;
            var handler = serviceProvider.GetKeyedService<ICommandHandler>(command);

            if (handler is null)
            {
                AnsiConsole.MarkupLine($"[{ColorPalette.Tertiary}]Command '{Markup.Escape(command)}' not found.[/]");
                return;
            }
            
            var handlerArgs = tokens.Skip(1).ToArray();

            handler.HandleAsync(handlerArgs).Wait();
            return;
        }
        else
        {
            PsCommandExecutor.ExecutePowerShell(input);
        }
    }

    
}
