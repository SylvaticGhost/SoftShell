using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using SoftShell.Infra;
using Spectre.Console;

namespace SoftShell.LLM;

public class GroqLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private const string Model = "llama-3.1-8b-instant";
    private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    public GroqLlmService(CredentialProvider credentialProvider)
    {
        _apiKey = credentialProvider.GetValue("GROQ_API_KEY");
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            AnsiConsole.MarkupLine("[rgb(190,89,133)]Critical error:[/] Credential GROQ_API_KEY is missing.");
            AnsiConsole.MarkupLine("Please set it by command [rgb(255,184,224)]/credential set -n GROQ_API_KEY -v {your-key}[/]");
        }

        _httpClient = new HttpClient();
    }

    private const string TranslatePrompt = """
                                           You are a PowerShell expert. The user will provide a Bash command with flags.
                                           Translate it to the closest idiomatic PowerShell equivalent.
                                           RETURN ONLY VALID JSON without markdown formatting (no ```json).
                                           Format: { "Command": "<PowerShell command>", "Explanation": "<Short explanation>" }
                                           """;

    public Task<LlmResponse?> TranslateBashAsync(string bashCommand) =>
        FetchFromLlmAsync(TranslatePrompt, bashCommand, "Analyzing and translating...");

    private const string GeneratePrompt = """
                                          You are a PowerShell expert. Generate a PowerShell command to solve the user's task.
                                          RETURN ONLY VALID JSON without markdown formatting.
                                          Format: { "Command": "<PowerShell command>", "Explanation": "<Short explanation>" }
                                          """;

    public Task<LlmResponse?> GenerateCommandAsync(string naturalLanguageQuery) =>
        FetchFromLlmAsync(GeneratePrompt, naturalLanguageQuery, "Find answer...");

    private async Task<LlmResponse?> FetchFromLlmAsync(string system, string user, string statusText)
    {
        if (string.IsNullOrWhiteSpace(_apiKey)) return null;

        LlmResponse? result = null;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.SquareCorners)
            .SpinnerStyle(new Style(new Color(236, 127, 169))) 
            .StartAsync($"[rgb(255,184,224)]{statusText}[/]", async _ =>
            {
                try
                {
                    var request = new ChatRequest(
                        Model,
                        [
                            new ChatMessage("system", system),
                            new ChatMessage("user", user)
                        ]
                    );

                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                    httpRequest.Content = JsonContent.Create(request, SoftShellJsonContext.Default.ChatRequest);

                    var response = await _httpClient.SendAsync(httpRequest);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errBody = await response.Content.ReadAsStringAsync();
                        throw new Exception($"LLM API returned {(int)response.StatusCode} {response.ReasonPhrase}: {errBody}");
                    }

                    var chatResponse = await response.Content.ReadFromJsonAsync(SoftShellJsonContext.Default.ChatResponse);

                    if (chatResponse?.Choices.Count > 0)
                    {
                        var rawText = chatResponse.Choices[0].Message.Content.Trim();
                        rawText = rawText.Replace("```json", "").Replace("```", "").Trim();

                        // Try to deserialize directly first
                        try
                        {
                            result = JsonSerializer.Deserialize(rawText, SoftShellJsonContext.Default.LlmResponse);
                        }
                        catch (JsonException)
                        {
                            // Attempt to extract JSON object from the text (first '{' .. last '}')
                            var first = rawText.IndexOf('{');
                            var last = rawText.LastIndexOf('}');
                            if (first >= 0 && last > first)
                            {
                                var jsonPart = rawText[first..(last + 1)];

                                // Sanitize invalid escape sequences like "\p" -> "\\p" so the parser won't fail
                                jsonPart = Regex.Replace(jsonPart, @"\\(?![""\/bnfrtu])", "\\\\");

                                try
                                {
                                    result = JsonSerializer.Deserialize(jsonPart, SoftShellJsonContext.Default.LlmResponse);
                                }
                                catch (JsonException jex)
                                {
                                    AnsiConsole.MarkupLine($"\n[rgb(190,89,133)]AI parse error:[/] {Markup.Escape(jex.Message)}");
                                    AnsiConsole.MarkupLine("[rgb(255,184,224)]Raw LLM output:[/]");
                                    AnsiConsole.WriteLine(rawText);
                                }
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("[rgb(190,89,133)]AI parse error:[/] Could not locate JSON object in model output.");
                                AnsiConsole.MarkupLine("[rgb(255,184,224)]Raw LLM output:[/]");
                                AnsiConsole.WriteLine(rawText);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"\n[rgb(190,89,133)]AI Error:[/] {Markup.Escape(ex.Message)}");
                }
            });

        return result;
    }
}
