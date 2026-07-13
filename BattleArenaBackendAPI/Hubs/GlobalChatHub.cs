using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace BattleArenaBackendAPI.Hubs
{
    /// <summary>
    /// Global chat hub mapped at /hub/global-chat.
    /// Clients call SendMessage(message); the server broadcasts
    /// ReceiveMessage(username, message) to every connected client.
    /// </summary>
    public class GlobalChatHub : Hub
    {
        private readonly IDatabase _redis;

        private const int MaxMessages = 5;
        private const int WindowSeconds = 10;

        public GlobalChatHub(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task SendMessage(string message)
        {
            // Prefer the authenticated identity; fall back to a generic name for anonymous connections.
            var username = Context.User?.Identity?.Name ?? "Anonymous";

            // Rate limiting: dùng Redis INCR + EXPIRE để đếm số tin nhắn trong cửa sổ 10 giây
            var rateLimitKey = $"chat_rate:{Context.ConnectionId}";

            var count = await _redis.StringIncrementAsync(rateLimitKey);
            if(count == 1)
            {
                // Chỉ set TTL cho lần đầu tiên để tránh reset timer mỗi lần gửi
                await _redis.KeyExpireAsync(rateLimitKey, TimeSpan.FromSeconds(WindowSeconds));
            }

            if (count > MaxMessages)
            {
                // Ném lỗi về client mà không broadcast tin nhắn
                throw new HubException($"Rate limit exceeded. You can send up to {MaxMessages} messages every {WindowSeconds} seconds.");
            }

                await Clients.All.SendAsync("ReceiveMessage", username, message);
        }
    }
}
