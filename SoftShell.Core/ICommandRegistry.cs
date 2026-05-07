namespace SoftShell.Core;

public interface ICommandRegistry
{
    IEnumerable<Command> GetAll();
    ICommandHandler? GetHandler(string commandName);

    Command? GetDefinition(string commandName);
}