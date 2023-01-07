using System.Threading.Tasks;
using Impostor.Api.Net;
using Impostor.Api.Net.Custom;
using Impostor.Hazel.Abstractions;
using Microsoft.Extensions.Logging;
using Reactor.Networking;

namespace Reactor.Impostor.Net;

public class Message255Reactor : ICustomRootMessage
{
    private readonly ILogger<Message255Reactor> _logger;

    public Message255Reactor(ILogger<Message255Reactor> logger)
    {
        _logger = logger;
    }

    public byte Id => byte.MaxValue;

    public ValueTask HandleMessageAsync(IClient client, IMessageReader reader, MessageType messageType)
    {
        var clientInfo = client.GetReactor();
        if (clientInfo == null)
        {
            return default;
        }

        var flag = (ReactorMessageFlags)reader.ReadByte();

        switch (flag)
        {
            default:
                _logger.LogWarning("Unknown reactor message flag {Flag}", flag);
                break;
        }

        return default;
    }
}
