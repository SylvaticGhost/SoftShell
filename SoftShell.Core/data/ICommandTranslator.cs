namespace SoftShell.Core.data;

public interface ICommandTranslator
{
    bool TryTranslate(string bashCommand, out string? psCommand);
}