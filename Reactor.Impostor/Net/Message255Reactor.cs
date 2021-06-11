using System.Threading.Tasks;
using Impostor.Api.Net;
using Impostor.Api.Net.Custom;
using Impostor.Api.Net.Messages;
using Microsoft.Extensions.Logging;
using Reactor.Impostor.Net.ReactorMessages;
using Reactor.Networking;

namespace Reactor.Impostor.Net
{
    public class Message255Reactor : ICustomRootMessage
    {
        private readonly ILogger<Message255Reactor> _logger;

        public Message255Reactor(ILogger<Message255Reactor> logger)
        {
            _logger = logger;
        }

        public byte Id => byte.MaxValue;

        public static void Deserialize(IMessageReader reader, out ReactorMessageFlags flag)
        {
            flag = (ReactorMessageFlags) reader.ReadByte();
        }

        public ValueTask HandleMessageAsync(IClient client, IMessageReader reader, MessageType messageType)
        {
            var clientInfo = client.GetReactor();
            if (clientInfo == null)
            {
                return default;
            }

            Deserialize(reader, out var flag);

            switch (flag)
            {
                case ReactorMessageFlags.ModDeclaration:
                {
                    Reactor01ModDeclaration.Deserialize(reader, out var netId, out var id, out var version, out var side);

                    clientInfo.Mods.Add(new Mod(netId, id, version, side));
                    break;
                }

                default:
                    _logger.LogWarning("Server received unknown flag {0}.", flag);
                    break;
            }

            return default;
        }
    }
}
