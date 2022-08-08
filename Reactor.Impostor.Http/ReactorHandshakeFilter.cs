using Impostor.Api.Games;
using Impostor.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Reactor.Networking;

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
        else if (headers.Count == 1)
        {
            var myClientInfo = this.BuildReactorClientInfo(headers[0]);
            if (myClientInfo == null)
            {
                this.logger.LogError("Couldn't parse ReactorClientHttp from string, \"{}\"", headers[0]);
                return game => false;
            }

            context.Response.Headers.Add("Client-Mods-Processed", "yes");

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
        else
        {
            this.logger.LogError("Client sent over {} headers, expected 0 or 1", headers.Count);
            return game => false;
        }

        throw new NotImplementedException();
    }

    private ReactorClientInfo? BuildReactorClientInfo(string str)
    {
        string[] parts = str.Split(';');
        if (parts.Length < 2)
        {
            this.logger.LogError("Expected a version and a mod count");
            return null;
        }

        if (parts[0] != "1")
        {
            this.logger.LogError("Expected version \"1\", got \"{}\"", parts[0]);
            return null;
        }

        var modCount = int.Parse(parts[1]);
        if (parts.Length - 2 != modCount)
        {
            this.logger.LogError("Expected \"{}\" mods, got \"{}\"", modCount, parts.Length - 2);
            return null;
        }

        var clientInfo = new ReactorClientInfo(ReactorProtocolVersion.Initial, modCount);
        for (uint i = 2; i < parts.Length; i++)
        {
            var modParts = parts[i].Split("=");
            if (modParts.Length != 2)
            {
                this.logger.LogError("Expected exactly one \"=\" in \"{}\"", parts[i]);
                return null;
            }

            clientInfo.Mods.Add(new Mod(i, modParts[0], modParts[1], PluginSide.Both));
        }

        return clientInfo;
    }
}
