using System;
using System.Threading.Tasks;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner;
using Impostor.Hazel.Abstractions;
using Reactor.Impostor.Net;
using Reactor.Networking;

namespace Reactor.Impostor.Rpcs;

public static class Extensions
{
    internal static Mod ReadMod(this IMessageReader reader, ReactorClientInfo clientInfo)
    {
        var netId = reader.ReadPackedUInt32();
        if (netId > 0)
        {
            return clientInfo.GetByNetId(netId);
        }

        var id = reader.ReadString();
        return clientInfo.GetModById(id);
    }

    internal static void Write(this IMessageWriter writer, Mod mod, ReactorClientInfo clientInfo)
    {
        if (mod.IsRequiredOnAllClients)
        {
            writer.WritePacked(clientInfo.GetNetId(mod));
        }
        else
        {
            writer.WritePacked(0);
            writer.Write(mod.Id);
        }
    }

    public static ValueTask SendReactorRpcAsync(this IInnerNetObject innerNetObject, IClient target, IReactorCustomRpc customRpc, IMessageWriter data, int? targetClientId = null)
    {
        var clientInfo = target.GetReactor();
        if (clientInfo == null)
        {
            throw new ArgumentException("Tried to send reactor rpc to vanilla client");
        }

        var targetMod = clientInfo.GetModById(customRpc.ModId);

        using var writer = innerNetObject.Game.StartRpc(innerNetObject.NetId, Rpc255Reactor.CallId, targetClientId);

        writer.Write(targetMod, clientInfo);
        writer.WritePacked(customRpc.Id);

        writer.StartMessage(0);
        writer.Write(data.ToByteArray(false));
        writer.EndMessage();

        writer.EndMessage();
        writer.EndMessage();

        return target.Connection!.SendAsync(writer);
    }
}
