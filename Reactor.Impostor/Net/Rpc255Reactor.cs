using System.Linq;
using System.Threading.Tasks;
using Impostor.Api;
using Impostor.Api.Net;
using Impostor.Api.Net.Custom;
using Impostor.Api.Net.Inner;
using Impostor.Hazel.Abstractions;
using Microsoft.Extensions.Logging;
using Reactor.Impostor.Rpcs;

namespace Reactor.Impostor.Net;

public class Rpc255Reactor : ICustomRpc
{
    private readonly ILogger<Rpc255Reactor> _logger;
    private readonly IReactorCustomRpcManager _manager;

    public Rpc255Reactor(ILogger<Rpc255Reactor> logger, IReactorCustomRpcManager manager)
    {
        _logger = logger;
        _manager = manager;
    }

    public const RpcCalls CallId = (RpcCalls)byte.MaxValue;
    public byte Id => byte.MaxValue;

    public async ValueTask<bool> HandleRpcAsync(IInnerNetObject innerNetObject, IClientPlayer sender, IClientPlayer? target, IMessageReader reader)
    {
        var clientInfo = sender.Client.GetReactor();

        if (clientInfo == null)
        {
            await sender.Client.ReportCheatAsync(new CheatContext(nameof(Rpc255Reactor)), "Client tried to send reactor custom rpc without completing handshake");
            return false;
        }

        var mod = reader.ReadMod(clientInfo);
        var rpcId = reader.ReadPackedUInt32();

        _logger.LogTrace("Received custom rpc: {RpcId} from mod {Mod}", rpcId, mod.Id);

        var messageReader = reader.ReadMessage();

        var customRpc = _manager.Rpcs.SingleOrDefault(x => x.ModId == mod.Id && x.Id == rpcId);
        if (customRpc != null)
        {
            return await customRpc.HandleAsync(innerNetObject, sender, target, messageReader.Copy());
        }

        return true;
    }
}
