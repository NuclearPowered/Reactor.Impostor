using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Plugins;
using Reactor.Impostor.Rpcs;

namespace Reactor.Impostor.Example
{
    [ImpostorPlugin("gg.reactor.impostor.example")]
    public class ExamplePlugin : PluginBase
    {
        private readonly IReactorCustomRpcManager _rpcManager;

        private MultiDisposable? _disposable;

        public ExamplePlugin(IReactorCustomRpcManager rpcManager)
        {
            _rpcManager = rpcManager;
        }

        public override ValueTask EnableAsync()
        {
            _disposable = new MultiDisposable(
                _rpcManager.Register<ExampleRpc>()
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
