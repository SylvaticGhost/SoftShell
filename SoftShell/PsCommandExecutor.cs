using System.Diagnostics;
using Spectre.Console;
using TextCopy;

namespace SoftShell;

public static class PsCommandExecutor
{
    private static readonly Style PromptHighlightStyle = new(new Color(0xBE, 0x59, 0x85));
    
    public static void HandleGeneratedCommand(string command, string title, string explanation)
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
                .HighlightStyle(PromptHighlightStyle)
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

    public static (string? output, string? error) ExecutePowerShellWithReturn(string command)
    {
        var currentDirectory = Environment.CurrentDirectory;
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
                WorkingDirectory = currentDirectory
            };

            using var process = Process.Start(startInfo);
            if (process == null) return (null, null);

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (output, error);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public static void ExecutePowerShell(string command)
    {
        try
        {
            var (output, error) = ExecutePowerShellWithReturn(command);

            if (!string.IsNullOrEmpty(error))
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(error)}[/]");
                return;
            }

            if (string.IsNullOrEmpty(output))
            {
                AnsiConsole.MarkupLine("\n");
                return;
            }

            var lines = output.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                string lastLine = lines[^1].Trim();
                if (Directory.Exists(lastLine))
                {
                    for (int i = 0; i < lines.Length - 1; i++)
                        Console.WriteLine(lines[i]);
                }
                else
                {
                    Console.Write(output);
                }
            }

            if (!string.IsNullOrEmpty(error)) 
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(error)}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[{ColorPalette.Primary}]Error executing command: {Markup.Escape(ex.Message)}[/]");
        }
    }
}