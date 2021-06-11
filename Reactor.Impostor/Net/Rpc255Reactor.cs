using System;
using System.Linq;
using System.Threading.Tasks;
using Impostor.Api;
using Impostor.Api.Net;
using Impostor.Api.Net.Custom;
using Impostor.Api.Net.Inner;
using Impostor.Api.Net.Messages;
using Microsoft.Extensions.Logging;
using Reactor.Impostor.Rpcs;

namespace Reactor.Impostor.Net
{
    public class Rpc255Reactor : ICustomRpc
    {
        private readonly ILogger<Rpc255Reactor> _logger;
        private readonly IMessageWriterProvider _writerProvider;
        private readonly IReactorCustomRpcManager _manager;

        public Rpc255Reactor(ILogger<Rpc255Reactor> logger, IMessageWriterProvider writerProvider, IReactorCustomRpcManager manager)
        {
            _logger = logger;
            _writerProvider = writerProvider;
            _manager = manager;
        }

        public byte Id => byte.MaxValue;

        public static void Deserialize(IMessageReader reader, out uint modNetId, out uint rpcId)
        {
            modNetId = reader.ReadPackedUInt32();
            rpcId = reader.ReadPackedUInt32();
        }

        public async ValueTask<bool> HandleRpcAsync(IInnerNetObject innerNetObject, IClientPlayer sender, IClientPlayer? target, IMessageReader reader)
        {
            var clientInfo = sender.Client.GetReactor();

            if (clientInfo == null)
            {
                await sender.Client.ReportCheatAsync(new CheatContext(nameof(Rpc255Reactor)), "Client tried to send reactor custom rpc without completing handshake");
                return false;
            }

            Deserialize(reader, out var modNetId, out var rpcId);

            var mod = clientInfo.Mods.Single(x => x.NetId == modNetId);

            _logger.LogTrace("Received custom rpc: {rpcId} from mod {mod}", rpcId, mod.Id);

            var relay = true;
            var messageReader = reader.ReadMessage();

            var customRpc = _manager.Rpcs.SingleOrDefault(x => x.ModId == mod.Id && x.Id == rpcId);
            if (customRpc != null)
            {
                if (!await customRpc.HandleAsync(innerNetObject, sender, target, messageReader.Copy()))
                {
                    relay = false;
                }
            }

            customRpc ??= new DummyReactorCustomRpc(mod.Id, rpcId);

            if (relay)
            {
                var game = innerNetObject.Game;

                var data = _writerProvider.Get();
                Buffer.BlockCopy(messageReader.Buffer, messageReader.Offset, data.Buffer, data.Position, messageReader.Length);
                data.Position += messageReader.Length;
                if (data.Position > data.Length) data.Length = data.Position;

                if (target != null)
                {
                    await innerNetObject.SendReactorRpcAsync(target.Client, customRpc, data, target.Client.Id);
                }
                else
                {
                    foreach (var targetPlayer in game.Players.Where(x => x != sender))
                    {
                        await innerNetObject.SendReactorRpcAsync(targetPlayer.Client, customRpc, data);
                    }
                }
            }

            return false;
        }

        private class DummyReactorCustomRpc : IReactorCustomRpc
        {
            public string ModId { get; }
            public uint Id { get; }

            public DummyReactorCustomRpc(string modId, uint id)
            {
                ModId = modId;
                Id = id;
            }

            public ValueTask<bool> HandleAsync(IInnerNetObject innerNetObject, IClientPlayer sender, IClientPlayer? target, IMessageReader reader)
            {
                throw new NotSupportedException();
            }
        }
    }
}
