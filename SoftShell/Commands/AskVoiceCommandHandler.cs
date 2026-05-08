using System.Diagnostics.CodeAnalysis;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using SoftShell.Core;
using SoftShell.LLM;
using Spectre.Console;

namespace SoftShell.Commands;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class AskVoiceCommandHandler(ILlmService llmService) : ICommandDefinedHandler
{
    public static Command Definition { get; } = new Command
    {
        Name = "ask-v",
        Description = "Voice request for suggesting PowerShell command",
        Aliases = ["av"],
        Flags = [
            new Flag { 
                Name = "--speech", 
                ShortName = "-s", 
                Description = "Play audion explanation" 
            }
        ]
    };

    public async Task HandleAsync(string[] args)
    {
        var arguments = string.Join(" ", args);
        string voiceQuery = GetVoiceInput();

        if (string.IsNullOrWhiteSpace(voiceQuery))
        {
            AnsiConsole.MarkupLine(
                "[rgb(190,89,133)]Error:[/] Failed ro recognize voice.");
            return;
        }

        AnsiConsole.MarkupLine($"[rgb(255,184,224)]Recognized:[/] [rgb(236,127,169)]{voiceQuery}[/]");

        var response = await llmService.GenerateCommandAsync(voiceQuery);

        if (response != null && !string.IsNullOrEmpty(response.Command))
        {
            bool useSpeech = arguments.Contains("-s", StringComparison.OrdinalIgnoreCase) || 
                             arguments.Contains("--speech", StringComparison.OrdinalIgnoreCase);

            if (useSpeech) SpeakText(response.Explanation);
            
            PsCommandExecutor.HandleGeneratedCommand(response.Command, "AI Suggested Command", response.Explanation);
        }
    }

    private static string GetVoiceInput()
    {
        SpeechRecognitionEngine? recognizer = null;
        try
        {
            recognizer = new SpeechRecognitionEngine();
            recognizer.LoadGrammar(new DictationGrammar());

            recognizer.SetInputToDefaultAudioDevice();
            AnsiConsole.MarkupLine($"[rgb(190,89,133)]Listening on {Markup.Escape("Default device")}...[/] (Ctrl+C to cancel/Timeout 10s)");

            var result = recognizer.Recognize(TimeSpan.FromSeconds(10));
            return result?.Text ?? string.Empty;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[rgb(190,89,133)]Microphone error/STT:[/] {ex.Message}");
            AnsiConsole.MarkupLine(
                "[rgb(255,184,224)]Warning: Verify that you have properly connected microphone.[/]");
            return string.Empty;
        }
        finally
        {
            recognizer?.Dispose();
        }
    }

    private static void SpeakText(string textToSpeak)
    {
        if (string.IsNullOrWhiteSpace(textToSpeak))
            return;

        try
        {
            using var synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();

            var installedVoices = synthesizer.GetInstalledVoices();
            if (installedVoices != null)
            {
                foreach (var v in installedVoices)
                {
                    try
                    {
                        var cultureName = v.VoiceInfo.Culture?.Name ?? "";

                        if (cultureName.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                        {
                            synthesizer.SelectVoice(v.VoiceInfo.Name);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[rgb(190,89,133)]TTS voice selection failed:[/] {Markup.Escape(ex.Message)}");
                    }
                }

                AnsiConsole.MarkupLine("[rgb(190,89,133)]..audio..[/]");
            }

            synthesizer.SpeakAsync(textToSpeak);
        }
        catch (PlatformNotSupportedException)
        {
            AnsiConsole.MarkupLine("[rgb(190,89,133)]TTS is not supported on this platform.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[rgb(190,89,133)]TTS error:[/] {Markup.Escape(ex.Message)}");
        }
    }
}