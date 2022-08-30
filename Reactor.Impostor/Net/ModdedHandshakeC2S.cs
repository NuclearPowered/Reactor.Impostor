using Impostor.Api.Net.Messages;
using Reactor.Networking;

namespace Reactor.Impostor.Net;

public static class ModdedHandshakeC2S
{
    public static void Deserialize(IMessageReader reader, out Mod[] mods)
    {
        var modCount = reader.ReadPackedInt32();
        mods = new Mod[modCount];

        for (var i = 0; i < modCount; i++)
        {
            var id = reader.ReadString();
            var version = reader.ReadString();
            var flags = (ModFlags)reader.ReadUInt16();
            var name = (flags & ModFlags.RequireOnAllClients) != 0 ? reader.ReadString() : null;

            mods[i] = new Mod(id, version, flags, name);
        }
    }
}
