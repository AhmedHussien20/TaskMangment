
namespace TaskMangment.Infrastructure.SignalR
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using System.Collections.Concurrent;
    using TaskMangment.Infrastructure.DataContext;

    /// <summary>
    /// Employee ids repeat across dbo and the training schema (and across trainee copies share one schema),
    /// so a connection is identified by schema + employee id, both read from the caller's token.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {

        public static readonly ConcurrentDictionary<string, List<string>> _onlineUsers
            = new ConcurrentDictionary<string, List<string>>();

        public static string ConnectionKey(string? schema, int userId)
        {
            var resolved = string.IsNullOrWhiteSpace(schema) ? OperationalSchema.Dbo : schema.Trim().ToLowerInvariant();
            return $"{resolved}:{userId}";
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            var isEmployee = !string.Equals(user?.FindFirst("Actor")?.Value, "Platform", StringComparison.OrdinalIgnoreCase);

            if (isEmployee && int.TryParse(user?.FindFirst("UserId")?.Value, out int userId))
            {
                var key = ConnectionKey(user!.FindFirst("Schema")?.Value, userId);
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

        public static bool IsUserOnline(string connectionKey)
        {
            return _onlineUsers.ContainsKey(connectionKey);
        }

        /// <summary>A snapshot of the user's connection ids, safe to enumerate while others connect.</summary>
        public static IReadOnlyList<string> ConnectionsFor(string connectionKey)
        {
            if (!_onlineUsers.TryGetValue(connectionKey, out var connections))
                return Array.Empty<string>();

            lock (connections)
                return connections.ToList();
        }
    }
}

