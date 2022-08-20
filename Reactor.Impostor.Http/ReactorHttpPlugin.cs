using Impostor.Api.Plugins;

namespace Reactor.Impostor.Http;

/// <summary>
/// Plugin that checks the Reactor HTTP handshake for Impostor.Http.
/// </summary>
[ImpostorPlugin("gg.reactor.http")]
[ImpostorDependency("gg.impostor.http", DependencyType.HardDependency)]
[ImpostorDependency("gg.reactor.impostor", DependencyType.HardDependency)]
public class ReactorHttpPlugin : PluginBase
{
}
