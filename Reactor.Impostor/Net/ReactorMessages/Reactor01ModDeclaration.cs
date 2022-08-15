using Impostor.Api.Net.Messages;

namespace Reactor.Impostor.Net.ReactorMessages;

public static class Reactor01ModDeclaration
{
    public static void Deserialize(IMessageReader reader, out uint netId, out string id, out string version, out PluginSide side)
    {
        netId = reader.ReadPackedUInt32();
        id = reader.ReadString();
        version = reader.ReadString();
        side = (PluginSide) reader.ReadByte();
    }
}