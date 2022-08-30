using System;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Client;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Messages;
using Impostor.Api.Utils;
using Microsoft.Extensions.Logging;
using Reactor.Impostor.Net;
using Reactor.Networking;

namespace Reactor.Impostor;

internal class HandshakeEventListener : IEventListener
{
    private readonly ILogger<HandshakeEventListener> _logger;
    private readonly IMessageWriterProvider _messageWriterProvider;
    private readonly IServerEnvironment _serverEnvironment;

    public HandshakeEventListener(ILogger<HandshakeEventListener> logger, IMessageWriterProvider messageWriterProvider, IServerEnvironment serverEnvironment)
    {
        _logger = logger;
        _messageWriterProvider = messageWriterProvider;
        _serverEnvironment = serverEnvironment;
    }

    [EventListener]
    public void OnClientConnectionEvent(IClientConnectionEvent e)
    {
        if (!ReactorHeader.TryDeserialize(e.HandshakeData, out var version))
        {
            return;
        }

        Mod[] mods;

        if (version >= ReactorProtocolVersion.V3)
        {
            ModdedHandshakeC2S.Deserialize(e.HandshakeData, out mods);
        }
        else
        {
            mods = Array.Empty<Mod>();
        }

        e.Connection.SetReactor(new ReactorClientInfo(version.Value, mods));

        _logger.LogTrace("New modded connection (protocol version: {ProtocolVersion}, mods: {@Mods})", version.Value, mods);
    }

    [EventListener]
    public async ValueTask OnClientConnectedEventAsync(IClientConnectedEvent e)
    {
        var clientInfo = e.Connection.GetReactor();

        if (clientInfo == null)
        {
            return;
        }

        if (clientInfo.Version != ReactorProtocolVersion.V3)
        {
            await e.Client.DisconnectAsync(DisconnectReason.Custom, "Unsupported Reactor.Networking protocol version");
            return;
        }

        using var writer = _messageWriterProvider.Get(MessageType.Reliable);
        ModdedHandshakeS2C.Serialize(writer, "Impostor", _serverEnvironment.Version, 0);
        await e.Connection.SendAsync(writer);
    }

    [EventListener]
    public void OnGamePlayerPreJoinEvent(IGamePlayerJoiningEvent e)
    {
        var client = e.Player.Client;
        var clientInfo = client.GetReactor();

        var host = e.Game.Host;

        if (host != null)
        {
            var hostMods = host.Client.GetReactor()?.Mods ?? Array.Empty<Mod>();
            var clientMods = clientInfo?.Mods ?? Array.Empty<Mod>();

            if (!Mod.Validate(clientMods, hostMods, out var reason))
            {
                e.JoinResult = GameJoinResult.CreateCustomError(reason);
            }
        }
    }
}
