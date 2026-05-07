using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SoftShell.Core;

namespace SoftShell.Infra;

internal interface ICommandRegistrator
{
    ICommandRegistrator AddHandling<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommandHandler>(
      )
        where TCommandHandler : class, ICommandHandler, ICommandHandlerDefinition;
}

file sealed class CommandRegistrator(IServiceCollection services) : ICommandRegistrator
{
    private readonly List<Command> _definitions = [];

    public ICommandRegistrator AddHandling<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommandHandler>()
        where TCommandHandler : class, ICommandHandler, ICommandHandlerDefinition
    {
        var definition = TCommandHandler.Definition;
        services.AddKeyedScoped<ICommandHandler, TCommandHandler>(definition.Name);
        
        foreach (var alias in definition.Aliases) 
            services.AddKeyedScoped<ICommandHandler, TCommandHandler>(alias);
        
        Console.WriteLine($"Adding handler for {definition.Name}");
        _definitions.Add(definition);
        return this;
    }

    public void BuildRegistry()
    {
        _definitions.AddRange(MetaCommands.Definitions);
        services.AddSingleton(new CommandDefinitionContainer(_definitions));
        services.AddSingleton<ICommandRegistry, CommandRegistry>();
    }
}

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services,
        Action<ICommandRegistrator> config)
    {
        var registrator = new CommandRegistrator(services);
        config(registrator);
        registrator.BuildRegistry();
        return services;
    }
}