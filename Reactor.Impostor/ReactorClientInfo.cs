using System.Collections.Generic;
using Reactor.Networking;

namespace Reactor.Impostor
{
    public class ReactorClientInfo
    {
        public ReactorClientInfo(ReactorProtocolVersion version, int modCount)
        {
            Version = version;
            ModCount = modCount;
            Mods = new HashSet<Mod>(modCount);
        }

        public ReactorProtocolVersion Version { get; }

        public int ModCount { get; }

        public ISet<Mod> Mods { get; }
    }
}
