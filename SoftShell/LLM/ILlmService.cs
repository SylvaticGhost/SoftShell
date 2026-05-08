namespace SoftShell.LLM;

public interface ILlmService
{
    Task<LlmResponse?> TranslateBashAsync(string bashCommand);
    Task<LlmResponse?> GenerateCommandAsync(string naturalLanguageQuery);
}