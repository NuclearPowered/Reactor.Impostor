using System;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Net.Custom;
using Impostor.Api.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Reactor.Impostor.Net;

namespace Reactor.Impostor
{
    [ImpostorPlugin("gg.reactor.impostor")]
    public class ReactorPlugin : PluginBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICustomMessageManager<ICustomRootMessage> _customRootMessageManager;
        private readonly ICustomMessageManager<ICustomRpc> _customRpcManager;

        private MultiDisposable? _disposable;

        public ReactorPlugin(IServiceProvider serviceProvider, ICustomMessageManager<ICustomRootMessage> customRootMessageManager, ICustomMessageManager<ICustomRpc> customRpcManager)
        {
            _serviceProvider = serviceProvider;
            _customRootMessageManager = customRootMessageManager;
            _customRpcManager = customRpcManager;
        }

        public override ValueTask EnableAsync()
        {
            _disposable = new MultiDisposable(
                _customRootMessageManager.Register(ActivatorUtilities.CreateInstance<Message255Reactor>(_serviceProvider)),
                _customRpcManager.Register(ActivatorUtilities.CreateInstance<Rpc255Reactor>(_serviceProvider))
            );

            return default;
        }

        public override ValueTask DisableAsync()
        {
            _disposable?.Dispose();

            return default;
        }
    }
}
