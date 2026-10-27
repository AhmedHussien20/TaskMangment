
namespace TaskMangment.Infrastructure.SignalR
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using System.Collections.Concurrent;
    using TaskMangment.Infrastructure.DataContext;

    /// <summary>
    /// Employee ids repeat across dbo and the training schema, so a connection is identified by schema + employee id,
    /// read from the caller's token. Training keys also carry the trainee copy, so a connection can only ever be
    /// addressed from inside the same copy.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {

        public static readonly ConcurrentDictionary<string, List<string>> _onlineUsers
            = new ConcurrentDictionary<string, List<string>>();

        /// <returns>null when a training key has no company, so nothing can be sent to it.</returns>
        public static string? ConnectionKey(string? schema, int? companyId, int userId)
        {
            var resolved = string.IsNullOrWhiteSpace(schema) ? OperationalSchema.Dbo : schema.Trim().ToLowerInvariant();
            if (resolved != OperationalSchema.Training)
                return $"{resolved}:{userId}";

            return companyId is > 0 ? $"{resolved}:{companyId}:{userId}" : null;
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            var isEmployee = !string.Equals(user?.FindFirst("Actor")?.Value, "Platform", StringComparison.OrdinalIgnoreCase);

            var key = isEmployee && int.TryParse(user?.FindFirst("UserId")?.Value, out int userId)
                ? ConnectionKey(
                    user!.FindFirst("Schema")?.Value,
                    int.TryParse(user.FindFirst("CompanyId")?.Value, out var companyId) ? companyId : null,
                    userId)
                : null;

            if (key != null)
            {
                var connectionId = Context.ConnectionId;

                _onlineUsers.AddOrUpdate(key,
                    new List<string> { connectionId },
                    (_, existing) =>
                    {
                        lock (existing)
                            existing.Add(connectionId);
                        return existing;
                    });

                Console.WriteLine($"  User {key} connected with ConnectionId: {connectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var kvp in _onlineUsers)
            {
                bool removed;
                bool empty;
                lock (kvp.Value)
                {
                    removed = kvp.Value.Remove(Context.ConnectionId);
                    empty = kvp.Value.Count == 0;
                }

                if (!removed)
                    continue;

                if (empty)
                    _onlineUsers.TryRemove(kvp.Key, out _);

                Console.WriteLine($"  User {kvp.Key} disconnected");
                break;
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static bool IsUserOnline(string? connectionKey)
        {
            return connectionKey != null && _onlineUsers.ContainsKey(connectionKey);
        }

        /// <summary>A snapshot of the user's connection ids, safe to enumerate while others connect.</summary>
        public static IReadOnlyList<string> ConnectionsFor(string? connectionKey)
        {
            if (connectionKey == null || !_onlineUsers.TryGetValue(connectionKey, out var connections))
                return Array.Empty<string>();

            lock (connections)
                return connections.ToList();
        }
    }
}

