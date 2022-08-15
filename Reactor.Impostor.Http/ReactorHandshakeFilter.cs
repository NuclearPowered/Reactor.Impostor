using Impostor.Api.Games;
using Impostor.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Reactor.Networking;

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
        var headers = context.Request.Headers["Client-Mods"];

        // If no header was included, this is a vanilla game
        if (headers.Count == 0)
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
                return reactor == null || !reactor.Mods.Any(x => x.Side == PluginSide.Both);
            };
        }

        if (headers.Count == 1)
        {
            var myClientInfo = BuildReactorClientInfo(headers[0]);
            if (myClientInfo == null)
            {
                _logger.LogError("Couldn't parse ReactorClientHttp from string, \"{}\"", headers[0]);
                return _ => false;
            }

            context.Response.Headers.Add("Client-Mods-Processed", "yes");

            return game =>
            {
                var hostClientInfo = game.Host?.Client.GetReactor();
                if (hostClientInfo == null)
                {
                    return false;
                }

                var hostMods = hostClientInfo.Mods.Where(m => m.Side == PluginSide.Both);
                return hostMods.Count() == myClientInfo.ModCount && hostMods.All(m => myClientInfo.Mods.Contains(m));
            };
        }

        _logger.LogError("Client sent over {} headers, expected 0 or 1", headers.Count);
        return _ => false;
    }

    private ReactorClientInfo? BuildReactorClientInfo(string str)
    {
        var parts = str.Split(';');
        if (parts.Length < 2)
        {
            _logger.LogError("Expected a version and a mod count");
            return null;
        }

        if (parts[0] != "1")
        {
            _logger.LogError("Expected version \"1\", got \"{}\"", parts[0]);
            return null;
        }

        var modCount = int.Parse(parts[1]);
        if (parts.Length - 2 != modCount)
        {
            _logger.LogError("Expected \"{}\" mods, got \"{}\"", modCount, parts.Length - 2);
            return null;
        }

        var clientInfo = new ReactorClientInfo(ReactorProtocolVersion.Initial, modCount);
        for (uint i = 2; i < parts.Length; i++)
        {
            var modParts = parts[i].Split("=");
            if (modParts.Length != 2)
            {
                _logger.LogError("Expected exactly one \"=\" in \"{}\"", parts[i]);
                return null;
            }

            clientInfo.Mods.Add(new Mod(i, modParts[0], modParts[1], PluginSide.Both));
        }

        return clientInfo;
    }
}
