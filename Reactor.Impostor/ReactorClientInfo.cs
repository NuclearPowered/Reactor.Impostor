using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Networking;

namespace Reactor.Impostor;

public class ReactorClientInfo
{
    private readonly Dictionary<string, Mod> _modById;
    private readonly Dictionary<uint, Mod> _mapByNetId;
    private readonly Dictionary<Mod, uint> _netIdMap;

    public ReactorClientInfo(ReactorProtocolVersion version, Mod[] mods)
    {
        Version = version;
        Mods = mods;
        _modById = mods.ToDictionary(m => m.Id, m => m);

        _mapByNetId = new Dictionary<uint, Mod>(mods.Length);
        _netIdMap = new Dictionary<Mod, uint>(mods.Length);

        uint netId = 1;
        foreach (var mod in mods)
        {
            if (!mod.IsRequiredOnAllClients) continue;

            _mapByNetId[netId] = mod;
            _netIdMap[mod] = netId;
            netId++;
        }
    }

    public ReactorProtocolVersion Version { get; }

    public IReadOnlyList<Mod> Mods { get; }

    public Mod GetModById(string id)
    {
        return _modById[id];
    }

    public Mod GetByNetId(uint netId)
    {
        return _mapByNetId[netId];
    }

    public uint GetNetId(Mod mod)
    {
        if (!mod.IsRequiredOnAllClients) throw new ArgumentException("Cannot get a net id for a mod without RequireOnAllClients flag", nameof(mod));
        return _netIdMap[mod];
    }
}
