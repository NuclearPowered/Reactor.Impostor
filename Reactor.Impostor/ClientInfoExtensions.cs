using System.Runtime.CompilerServices;
using Impostor.Api.Net;

namespace Reactor.Impostor;

public static class ClientInfoExtensions
{
    private static ConditionalWeakTable<IHazelConnection, ReactorClientInfo> Table { get; } = new ConditionalWeakTable<IHazelConnection, ReactorClientInfo>();

    public static ReactorClientInfo? GetReactor(this IClient client)
    {
        return client.Connection?.GetReactor();
    }

    public static ReactorClientInfo? GetReactor(this IHazelConnection connection)
    {
        if (Table.TryGetValue(connection, out var clientInfo))
        {
            return clientInfo;
        }

        return null;
    }

    internal static void SetReactor(this IHazelConnection connection, ReactorClientInfo clientInfo)
    {
        Table.AddOrUpdate(connection, clientInfo);
    }
}