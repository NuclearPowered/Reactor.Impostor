using Impostor.Api.Plugins;
using Impostor.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Reactor.Impostor.Http;
/**
 * <summary>
 * Register the ReactorHandshakeFilter.
 * </summary>
 */
public class ReactorHttpPluginStartup : IPluginStartup
{
    /// <inheritdoc/>
    public void ConfigureHost(IHostBuilder host)
    {
        // Not used
    }

    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IListingFilter, ReactorHandshakeFilter>();
    }
}
