using System.Collections.ObjectModel;
using SoftShell.Core;

namespace SoftShell.Infra;

internal sealed record CommandDefinitionContainer(List<Command> Commands);