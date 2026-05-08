using System.Text;
using SoftShell.Core;
using Spectre.Console;
using SoftShell;

namespace SoftShell.Infra;

public class LivePrompt(ICommandRegistry commandRegistry)
{
    private int _ctrlCCount = 0;
    private static readonly List<string> History = [];

    public string? ReadLine(string currentDirectory)
    {
        AnsiConsole.Markup($"[{ColorPalette.Primary}] [[{Markup.Escape(currentDirectory)}]][/] [{ColorPalette.Secondary}]>[/] ");
        
        int startLeft = Console.CursorLeft;
        int startTop = Console.CursorTop;
        
        StringBuilder buffer = new StringBuilder();
        int lastSuggestionCount = 0;
        int historyIndex = History.Count;

        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) && keyInfo.Key == ConsoleKey.C)
            {
                ClearSuggestions(startTop, lastSuggestionCount);
                _ctrlCCount++;
                if (_ctrlCCount >= 2)
                {
                    return null;
                }

                AnsiConsole.MarkupLine($"[{ColorPalette.Tertiary}]^C[/]");
                return string.Empty;
            }

            _ctrlCCount = 0;

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                ClearSuggestions(startTop, lastSuggestionCount);
                Console.WriteLine();
                string result = buffer.ToString();
                if (!string.IsNullOrWhiteSpace(result) && (History.Count == 0 || History[^1] != result))
                {
                    History.Add(result);
                }
                return result;
            }

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                if (historyIndex > 0)
                {
                    historyIndex--;
                    ReplaceBuffer(startLeft, startTop, buffer, History[historyIndex]);
                }
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                if (historyIndex < History.Count - 1)
                {
                    historyIndex++;
                    ReplaceBuffer(startLeft, startTop, buffer, History[historyIndex]);
                }
                else if (historyIndex == History.Count - 1)
                {
                    historyIndex++;
                    ReplaceBuffer(startLeft, startTop, buffer, "");
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0)
                {
                    buffer.Remove(buffer.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else if (keyInfo.Key == ConsoleKey.Tab)
            {
                var suggestions = GetSuggestions(buffer.ToString());
                if (suggestions.Any())
                {
                    var first = suggestions.First();
                    
                    // Clear current line on screen
                    Console.SetCursorPosition(startLeft, startTop);
                    Console.Write(new string(' ', buffer.Length));
                    Console.SetCursorPosition(startLeft, startTop);
                    
                    buffer.Clear();
                    buffer.Append(Command.Prefix + first.Name + " ");
                    
                    AnsiConsole.Markup($"[{ColorPalette.Secondary}]{Markup.Escape(buffer.ToString())}[/]");
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                buffer.Append(keyInfo.KeyChar);
                AnsiConsole.Markup($"[{ColorPalette.Secondary}]{Markup.Escape(keyInfo.KeyChar.ToString())}[/]");
            }

            ClearSuggestions(startTop, lastSuggestionCount);
            lastSuggestionCount = ShowSuggestions(buffer.ToString(), startTop);
        }
    }

    private static void ReplaceBuffer(int startLeft, int startTop, StringBuilder buffer, string newValue)
    {
        Console.SetCursorPosition(startLeft, startTop);
        Console.Write(new string(' ', buffer.Length));
        Console.SetCursorPosition(startLeft, startTop);
        
        buffer.Clear();
        buffer.Append(newValue);
        AnsiConsole.Markup($"[{ColorPalette.Secondary}]{Markup.Escape(buffer.ToString())}[/]");
    }

    private List<Command> GetSuggestions(string input)
    {
        if (!input.StartsWith(Command.Prefix)) return [];

        return commandRegistry.GetAll()
            .Where(c => 
                (Command.Prefix + c.Name).StartsWith(input, StringComparison.OrdinalIgnoreCase) ||
                c.Aliases.Any(a => (Command.Prefix + a).StartsWith(input, StringComparison.OrdinalIgnoreCase)))
            .Take(5)
            .ToList();
    }

    private int ShowSuggestions(string input, int promptTop)
    {
        var suggestions = GetSuggestions(input);
        if (suggestions.Count == 0) return 0;

        int currentLeft = Console.CursorLeft;
        int currentTop = Console.CursorTop;

        int maxNameLength = suggestions.Max(s => s.Name.Length + Command.Prefix.Length);

        int shownCount = 0;
        for (int i = 0; i < suggestions.Count; i++)
        {
            int row = promptTop + 1 + i;
            if (row >= Console.BufferHeight) break;
            
            var s = suggestions[i];
            Console.SetCursorPosition(0, row);
            AnsiConsole.Markup($"[{ColorPalette.Secondary}]{Command.Prefix}{Markup.Escape(s.Name).PadRight(maxNameLength)}[/] [{ColorPalette.Tertiary}]- {Markup.Escape(s.Description)}[/]");
            shownCount++;
        }

        Console.SetCursorPosition(currentLeft, currentTop);
        return shownCount;
    }

    private void ClearSuggestions(int promptTop, int count)
    {
        if (count <= 0) return;

        int currentLeft = Console.CursorLeft;
        int currentTop = Console.CursorTop;

        for (int i = 0; i < count; i++)
        {
            int row = promptTop + 1 + i;
            if (row >= Console.BufferHeight) break;
            
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', Console.WindowWidth - 1));
        }

        Console.SetCursorPosition(currentLeft, currentTop);
    }
}
