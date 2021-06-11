using System;
using System.Collections.Generic;
using System.Linq;
using Impostor.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Reactor.Impostor.Rpcs
{
    internal class ReactorCustomRpcManager : IReactorCustomRpcManager
    {
        private readonly IServiceProvider _serviceProvider;

        public ReactorCustomRpcManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public List<IReactorCustomRpc> Rpcs { get; } = new List<IReactorCustomRpc>();

        IReadOnlyList<IReactorCustomRpc> IReactorCustomRpcManager.Rpcs => Rpcs;

        public IDisposable Register(IReactorCustomRpc customRpc)
        {
            if (Rpcs.Any(x => x.ModId == customRpc.ModId && x.Id == customRpc.Id))
            {
                throw new ImpostorException("Custom rpc with that id and mod id was already registered");
            }

            Rpcs.Add(customRpc);
            return new UnregisterDisposable(this, customRpc);
        }

        public IDisposable Register<T>() where T : IReactorCustomRpc
        {
            if (Rpcs.Any(x => x.GetType() == typeof(T)))
            {
                throw new ImpostorException("Reactor custom rpc with that type was already registered");
            }

            return Register(ActivatorUtilities.CreateInstance<T>(_serviceProvider));
        }

        public T Get<T>() where T : IReactorCustomRpc
        {
            return Rpcs.OfType<T>().Single();
        }

        private class UnregisterDisposable : IDisposable
        {
            private readonly ReactorCustomRpcManager _manager;
            private readonly IReactorCustomRpc _rpc;

            private bool _disposed;

            public UnregisterDisposable(ReactorCustomRpcManager manager, IReactorCustomRpc rpc)
            {
                _manager = manager;
                _rpc = rpc;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("Tried to dispose already disposed object");
                }

                _manager.Rpcs.Remove(_rpc);
                _disposed = true;
            }
        }
    }
}
