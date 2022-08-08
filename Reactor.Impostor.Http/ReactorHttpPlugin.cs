using Impostor.Api.Plugins;

namespace Reactor.Impostor.Http;

/// <summary>
/// Plugin that checks the Reactor HTTP handshake for Impostor.Http.
/// </summary>
[ImpostorPlugin("at.duikbo.http.reactor")]
[ImpostorDependency("at.duikbo.http", DependencyType.HardDependency)]
[ImpostorDependency("gg.reactor.impostor", DependencyType.HardDependency)]
public class ReactorHttpPlugin : PluginBase
{
}
