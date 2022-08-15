using System;
using System.Collections.Generic;

namespace Reactor.Impostor.Rpcs;

public interface IReactorCustomRpcManager
{
    IReadOnlyList<IReactorCustomRpc> Rpcs { get; }

    IDisposable Register(IReactorCustomRpc customRpc);
    IDisposable Register<T>() where T : IReactorCustomRpc;

    T Get<T>() where T : IReactorCustomRpc;
}