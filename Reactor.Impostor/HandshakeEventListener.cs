using System;
using System.Linq;
using System.Text;
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

namespace Reactor.Impostor
{
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
            if (!ModdedHandshakeC2S.Deserialize(e.HandshakeData, out var protocolVersion, out var modCount))
            {
                return;
            }

            e.Connection.SetReactor(new ReactorClientInfo(protocolVersion.Value, modCount.Value));

            _logger.LogTrace("New modded connection (protocol version: {protocolVersion}, mods: {modCount})", protocolVersion, modCount);
        }

        [EventListener]
        public async ValueTask OnClientConnectedEventAsync(IClientConnectedEvent e)
        {
            var clientInfo = e.Connection.GetReactor();

            if (clientInfo == null)
            {
                return;
            }

            if (clientInfo.Version < ReactorProtocolVersion.Initial || clientInfo.Version > ReactorProtocolVersion.Latest)
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

            if (clientInfo != null && clientInfo.Mods.Count != clientInfo.ModCount)
            {
                e.JoinResult = GameJoinResult.CreateCustomError("The modded handshake is corrupted");
                return;
            }

            if (host != null)
            {
                var hostMods = host.Client.GetReactor()?.Mods;
                var clientMods = clientInfo?.Mods;

                var clientMissing = hostMods != null
                    ? hostMods.Where(mod => mod.Side == PluginSide.Both && (clientMods == null || !clientMods.Contains(mod))).ToArray()
                    : Array.Empty<Mod>();

                var hostMissing = clientMods != null
                    ? clientMods.Where(mod => mod.Side == PluginSide.Both && (hostMods == null || !hostMods.Contains(mod))).ToArray()
                    : Array.Empty<Mod>();

                if (clientMissing.Any() || hostMissing.Any())
                {
                    var message = new StringBuilder();

                    if (clientMissing.Any())
                    {
                        message.Append("You are missing: ");
                        message.AppendJoin(", ", clientMissing.Select(x => $"{x.Id} ({x.Version})"));
                        message.AppendLine();
                    }

                    if (hostMissing.Any())
                    {
                        message.Append("Host is missing: ");
                        message.AppendJoin(", ", hostMissing.Select(x => $"{x.Id} ({x.Version})"));
                        message.AppendLine();
                    }

                    e.JoinResult = GameJoinResult.CreateCustomError(message.ToString());
                }
            }
        }
    }
}
