using Impostor.Api.Games;
using Impostor.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Reactor.Impostor.Http;
/**
 * <summary>
 * Check if players have the same mods using the Reactor Handshake.
 * </summary>
 */
public class ReactorHandshakeFilter : IListingFilter
{
    private readonly ILogger<ReactorHandshakeFilter> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactorHandshakeFilter"/> class.
    /// </summary>
    /// <param name="logger">Logger for this filter.</param>
    public ReactorHandshakeFilter(ILogger<ReactorHandshakeFilter> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc/>
    public Func<IGame, bool> GetFilter(HttpContext context)
    {
        var myClientInfo = context.Features.Get<ReactorClientInfo>();

        if (myClientInfo == null)
        {
            // If no header was included, this is a vanilla game
            return game =>
            {
                var host = game.Host;
                if (host == null)
                {
                    return true;
                }

                var reactor = host.Client.GetReactor();

                // Only allow if no handshake was present or only client side mods are present
                return reactor == null || !reactor.Mods.Any(x => x.Side == PluginSide.Both);
            };
        }
        else
        {
            // Otherwise check for modded games with compatible mods
            return game =>
            {
                var host = game.Host;
                if (host == null)
                {
                    return false;
                }

                var hostClientInfo = host.Client.GetReactor();
                if (hostClientInfo == null)
                {
                    return false;
                }

                var hostMods = hostClientInfo.Mods.Where(m => m.Side == PluginSide.Both);
                return hostMods.Count() == myClientInfo.ModCount && hostMods.All(m => myClientInfo.Mods.Contains(m));
            };
        }
    }
}
