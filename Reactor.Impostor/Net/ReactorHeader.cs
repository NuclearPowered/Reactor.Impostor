using System.Diagnostics.CodeAnalysis;
using Impostor.Hazel.Abstractions;
using Reactor.Networking;

namespace Reactor.Impostor.Net;

public static class ReactorHeader
{
    private const ulong MAGIC = 0x72656163746f72; // "reactor" in ascii, 7 bytes

    public static int Size => sizeof(ulong);

    public static bool TryDeserialize(IMessageReader reader, [NotNullWhen(true)] out ReactorProtocolVersion? version)
    {
        if (reader.Length >= reader.Position + Size)
        {
            var value = reader.ReadUInt64();
            var magic = value >> 8;

            if (magic == MAGIC)
            {
                version = (ReactorProtocolVersion)(value & 0xFF);
                return true;
            }
        }

        version = null;
        return false;
    }
}
