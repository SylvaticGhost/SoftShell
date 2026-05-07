Project Context: SoftShell
1. Core Identity & Vision
   Project Name: SoftShell

Concept: A "Bash-to-PowerShell" translation CLI and AI assistant. The anti-PowerShell: lightweight, text-first, and aesthetically driven.

Visual Style: Anime aesthetic, cyberpunk/kawaii terminal UI (Mecha-inspired HUDs), heavily utilizing square/blocky geometry for all UI elements.

Tech Stack: .NET 10.

UI Framework: Spectre.Console (version managed via Central Package Management).

2. Color Palette & Styling Rules
   The application uses a strict 4-color custom palette. It is designed to maintain contrast and readability across both light and dark terminal themes.

Use Spectre.Console's RGB markup syntax [rgb(R,G,B)]text[/] or HEX [#HEX]text[/].

Palette Priority (Highest to Lowest) (is defined as c# code in "/SoftShell/ColorPalette.cs":

Priority 1 (Primary / Core Elements): rgb(190, 89, 133) / #BE5985 (Dark Pink)

Usage: REPL input prompt (>), primary borders, critical errors, main headers, and active selections.

Priority 2 (Secondary / Accents): rgb(236, 127, 169) / #EC7FA9 (Medium Pink)

Usage: Command outputs, highlighted keywords, status spinners, and secondary panel titles.

Priority 3 (Tertiary / Muted Information): rgb(255, 184, 224) / #FFB8E0 (Light Pink)

Usage: Explanations, descriptions, secondary text, and inactive menu options.

Priority 4 (Background / Subtle details): rgb(255, 237, 250) / #FFEDFA (Lightest Pink)

Usage: Very subtle separators, background highlights (if applicable), or placeholder text. Use sparingly to avoid washing out the UI.

3. Typography & UI Geometry (The "Square Anime" Aesthetic)
   To achieve the "Square Anime / Mecha HUD" look within the terminal, all Spectre.Console widgets must adhere to strict geometric rules:

Borders: NEVER use rounded borders. Always use BoxBorder.Square, BoxBorder.Heavy, or BoxBorder.Double.

Figlet/ASCII Art: For the main logo and large text, use blocky/square Figlet fonts (e.g., "Rectangles", "Banner3-D", or "Cyberlarge"). Avoid cursive or rounded ASCII fonts.

Panels: All data blocks (e.g., LLM responses, command explanations) must be wrapped in Panel objects with sharp corners.

Separators: Use Rule widgets with a straight, solid line style (RuleStyle.Square or similar) to separate execution blocks.

4. REPL Interface Guidelines
   When generating code for the REPL loop, follow this structure:

Prompt Line: The prompt must look sharp and minimal.

Example: [rgb(190,89,133)][SoftShell] [[C:\Current\Path]][/] [rgb(236,127,169)]>[/]

Menus: When prompting the user for actions (Execute/Copy/Reject), use SelectionPrompt<T>.

Set the highlight style to the Priority 1 color (rgb(190,89,133)).

Use a square pointer, like > or ■.

Status Indicators: When waiting for the LLM or executing a command, use AnsiConsole.Status(). The spinner should be geometric (e.g., Spinner.Known.SquareCorners or Spinner.Known.Dots).

5. Code Generation Constraints
   Always prioritize the designated RGB colors over standard console colors (do not use [red], [blue], etc.).

Ensure all Spectre.Console markup is properly escaped if user input is being displayed.

Keep the UI clean. Anime interfaces are often dense with information but highly structured. Use tables (Table) with TableBorder.Square to align complex outputs.