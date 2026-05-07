using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SoftShell.Commands;
using SoftShell.Core;
using SoftShell.Core.data;
using SoftShell.Infra;
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
        services.AddTransient<LivePrompt>();
        services.AddCommandHandlers(r =>
        {
            r.AddHandling<HelpCommand>();
            r.AddHandling<TranslateCommandHandler>();
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
        AnsiConsole.MarkupLine($"[{ColorPalette.Tertiary}]Welcome back, Pilot. All systems operational.[/]");
        AnsiConsole.WriteLine();

        // REPL Loop
        while (true)
        {
            string input = livePrompt.ReadLine(_currentDirectory);

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

    private static string ShowPrompt()
    {
        AnsiConsole.Markup($"[{ColorPalette.Primary}] [[{Markup.Escape(_currentDirectory)}]][/] [{ColorPalette.Secondary}]>[/] ");
        return Console.ReadLine() ?? string.Empty;
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
            // Direct PowerShell execution
            ExecutePowerShell(input);
        }
    }

    private static void HandleGeneratedCommand(string command, string title, string explanation)
    {
        AnsiConsole.WriteLine();
        var panel = new Panel(new Markup($"[{ColorPalette.Secondary}]{Markup.Escape(command)}[/]\n\n[{ColorPalette.Tertiary}]{Markup.Escape(explanation)}[/]"))
        {
            Header = new PanelHeader($"[{ColorPalette.Primary}]{title}[/]"),
            Border = BoxBorder.Square,
            Padding = new Padding(1, 1, 1, 1)
        };
        AnsiConsole.Write(panel);

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{ColorPalette.Primary}]Action required:[/]")
                .HighlightStyle(new Style(foreground: Color.FromInt32(0xBE5985)))
                .AddChoices("Execute", "Copy", "Reject"));

        switch (choice)
        {
            case "Execute":
                ExecutePowerShell(command);
                break;
            case "Copy":
                ClipboardService.SetText(command);
                AnsiConsole.MarkupLine($"[{ColorPalette.Tertiary}]Command copied to clipboard.[/]");
                break;
            case "Reject":
                AnsiConsole.MarkupLine($"[{ColorPalette.Tertiary}]Command rejected.[/]");
                break;
        }
    }

    private static void ExecutePowerShell(string command)
    {
        try
        {
            string augmentedCommand = $"{command}; (Get-Location).Path";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -Command \"{augmentedCommand.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _currentDirectory
            };

            using var process = Process.Start(startInfo);
            if (process == null) return;

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // Extract the new path from the last line of output
            var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                string lastLine = lines[^1].Trim();
                if (Directory.Exists(lastLine))
                {
                    _currentDirectory = lastLine;
                    // Print everything except the last line (the path)
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        Console.WriteLine(lines[i]);
                    }
                }
                else
                {
                    Console.Write(output);
                }
            }

            if (!string.IsNullOrEmpty(error))
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(error)}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[{ColorPalette.Primary}]Error executing command: {Markup.Escape(ex.Message)}[/]");
        }
    }
}
