using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Reactor.Networking;

namespace Reactor.Impostor.Http;

/// <summary>
/// Add a Client-Mods-Processed header to all requests with a valid header.
/// </summary>
public class ClientModsHeader : IMiddleware
{
    private readonly ILogger<ClientModsHeader> _logger;

    /// <param name="logger">Logger for handshake issues</param>
    public ClientModsHeader(ILogger<ClientModsHeader> logger)
    {
        this._logger = logger;
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var clientInfo = GetReactorClientInfo(context.Request.Headers["Client-Mods"]);

        if (clientInfo != null)
        {
            context.Response.Headers.Add("Client-Mods-Processed", "yes");
            context.Features.Set(clientInfo);
        }

        await next(context);
    }

    private ReactorClientInfo? GetReactorClientInfo(StringValues headers)
    {
        if (headers.Count == 1)
        {
            return BuildReactorClientInfo(headers[0]);
        }
        else if (headers.Count > 1)
        {
            this._logger.LogError("Client sent over {} headers, expected 0 or 1", headers.Count);
        }
        return null;
    }

    private ReactorClientInfo? BuildReactorClientInfo(string str)
    {
        var parts = str.Split(';');
        if (parts.Length < 2)
        {
            this._logger.LogError("Expected a version and a mod count");
            return null;
        }

        if (parts[0] != "1")
        {
            this._logger.LogError("Expected version \"1\", got \"{}\"", parts[0]);
            return null;
        }

        var modCount = int.Parse(parts[1]);
        if (parts.Length - 2 != modCount)
        {
            this._logger.LogError("Expected \"{}\" mods, got \"{}\"", modCount, parts.Length - 2);
            return null;
        }

        var clientInfo = new ReactorClientInfo(ReactorProtocolVersion.Initial, modCount);
        for (uint i = 2; i < parts.Length; i++)
        {
            var modParts = parts[i].Split("=");
            if (modParts.Length != 2)
            {
                this._logger.LogError("Expected exactly one \"=\" in \"{}\"", parts[i]);
                return null;
            }

            clientInfo.Mods.Add(new Mod(i, modParts[0], modParts[1], PluginSide.Both));
        }

        return clientInfo;
    }
}
