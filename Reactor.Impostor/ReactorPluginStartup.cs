using Impostor.Api.Events;
using Impostor.Api.Http;
using Impostor.Api.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Reactor.Impostor.Http;
using Reactor.Impostor.Rpcs;

namespace Reactor.Impostor;

public class ReactorPluginStartup : IPluginHttpStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventListener, HandshakeEventListener>();
        services.AddSingleton<IReactorCustomRpcManager, ReactorCustomRpcManager>();

        services.AddSingleton<IListingFilter, ReactorHandshakeFilter>();
        services.AddSingleton<ClientModsHeader>();
    }

    public void ConfigureWebApplication(IApplicationBuilder builder)
    {
        builder.UseMiddleware<ClientModsHeader>();
    }

    public void ConfigureHost(IHostBuilder host)
    {
    }
}
