using System.Collections.Generic;
using Reactor.Networking;

namespace Reactor.Impostor
{
    public class ReactorClientInfo
    {
        public ReactorClientInfo(ReactorProtocolVersion version, int modCount)
        {
            Version = version;
            Mods = new HashSet<Mod>(modCount);
        }

        public ReactorProtocolVersion Version { get; }

        public ISet<Mod> Mods { get; }
    }
}
