using System;
using System.Linq;
using System.Threading.Tasks;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner;
using Impostor.Api.Net.Messages;

namespace Reactor.Impostor.Rpcs
{
    public static class Extensions
    {
        public static ValueTask SendReactorRpcAsync(this IInnerNetObject innerNetObject, IClient target, IReactorCustomRpc customRpc, IMessageWriter data, int? targetClientId = null)
        {
            var clientInfo = target.GetReactor();
            if (clientInfo == null)
            {
                throw new ArgumentException("Tried to send reactor rpc to vanilla client");
            }

            var targetMod = clientInfo.Mods.Single(x => x.Id == customRpc.ModId);

            using var writer = innerNetObject.Game.StartRpc(innerNetObject.NetId, (RpcCalls) 255, targetClientId);

            writer.WritePacked(targetMod.NetId);
            writer.WritePacked(customRpc.Id);

            writer.StartMessage(0);
            writer.Write(data.ToByteArray(false));
            writer.EndMessage();

            writer.EndMessage();
            writer.EndMessage();

            return target.Connection!.SendAsync(writer);
        }
    }
}
