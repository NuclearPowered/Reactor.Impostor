using Impostor.Api.Events;
using Impostor.Api.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Reactor.Impostor.Rpcs;

namespace Reactor.Impostor
{
    public class ReactorPluginStartup : IPluginStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEventListener, HandshakeEventListener>();
            services.AddSingleton<IReactorCustomRpcManager, ReactorCustomRpcManager>();
        }

        public void ConfigureHost(IHostBuilder host)
        {
        }
    }
}
