using Impostor.Api.Games;
using Impostor.Api.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Reactor.Impostor.Http;

/// <summary>
/// Check if players have the same mods using the Reactor Handshake.
/// </summary>
public class ReactorHandshakeFilter : IListingFilter
{
    private readonly ILogger<ReactorHandshakeFilter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactorHandshakeFilter"/> class.
    /// </summary>
    /// <param name="logger">Logger for this filter.</param>
    public ReactorHandshakeFilter(ILogger<ReactorHandshakeFilter> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Func<IGame, bool> GetFilter(HttpContext context)
    {
        var myClientInfo = context.Features.Get<ReactorClientInfo>();

        if (myClientInfo == null)
        {
            return game =>
            {
                var host = game.Host;
                if (host == null)
                {
                    return true;
                }

                var reactor = host.Client.GetReactor();

                // Only allow if no handshake was present or only client side mods are present
                return reactor == null || !reactor.Mods.Any(x => x.IsRequiredOnAllClients);
            };
        }

        return game =>
        {
            var hostClientInfo = game.Host?.Client.GetReactor();
            if (hostClientInfo == null)
            {
                return false;
            }

            var hostMods = hostClientInfo.Mods.Where(m => m.IsRequiredOnAllClients).ToArray();
            return hostMods.Length == myClientInfo.Mods.Count && hostMods.All(m => myClientInfo.Mods.Contains(m));
        };
    }
}
