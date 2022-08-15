using System.Linq;
using System.Threading.Tasks;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner;
using Impostor.Api.Net.Messages;

namespace Reactor.Impostor.Rpcs;

public interface IReactorCustomRpc
{
    string ModId { get; }
    uint Id { get; }

    public ValueTask<bool> HandleAsync(IInnerNetObject innerNetObject, IClientPlayer sender, IClientPlayer? target, IMessageReader reader);
}

public abstract class ReactorCustomRpc<T> : IReactorCustomRpc where T : IInnerNetObject
{
    public abstract string ModId { get; }
    public abstract uint Id { get; }

    public abstract ValueTask<bool> HandleAsync(T player, IClientPlayer sender, IClientPlayer? target, IMessageReader reader);

    public ValueTask<bool> HandleAsync(IInnerNetObject innerNetObject, IClientPlayer sender, IClientPlayer? target, IMessageReader reader)
    {
        return HandleAsync((T) innerNetObject, sender, target, reader);
    }

    protected async ValueTask SendAsync(T innerNetObject, IMessageWriter data, int? targetClientId = null)
    {
        foreach (var target in innerNetObject.Game.Players.Where(x => targetClientId == null || x.Client.Id == targetClientId))
        {
            await innerNetObject.SendReactorRpcAsync(target.Client, this, data, targetClientId);
        }
    }
}