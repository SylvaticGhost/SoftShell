using Microsoft.Extensions.DependencyInjection;
using SoftShell.Core;

namespace SoftShell.Infra;

internal sealed class CommandRegistry(CommandDefinitionContainer container, IServiceProvider serviceProvider)
    : ICommandRegistry
{
    public IEnumerable<Command> GetAll() => container.Commands;

    public ICommandHandler? GetHandler(string commandName) =>
        serviceProvider.GetKeyedService<ICommandHandler>(commandName);

    public Command? GetDefinition(string commandName) => container.Commands.FirstOrDefault(c => c.Name == commandName);
}