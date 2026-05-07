namespace SoftShell.Core;

public interface ICommandHandler
{
    public Task HandleAsync(string[] args);
}

public interface ICommandHandlerDefinition
{
    static abstract Command Definition { get; }
}

public interface ICommandDefinedHandler : ICommandHandler, ICommandHandlerDefinition;

