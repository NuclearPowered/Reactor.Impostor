using System.Diagnostics.CodeAnalysis;
using Impostor.Api.Net.Messages;
using Reactor.Networking;

namespace Reactor.Impostor.Net
{
    public static class ModdedHandshakeC2S
    {
        public static bool Deserialize(IMessageReader reader, [NotNullWhen(true)] out ReactorProtocolVersion? version, [NotNullWhen(true)] out int? modCount)
        {
            if (reader.Length > reader.Position)
            {
                version = (ReactorProtocolVersion) reader.ReadByte();
                modCount = reader.ReadPackedInt32();

                return true;
            }

            version = null;
            modCount = null;

            return false;
        }
    }
}
