using Microsoft.AspNetCore.SignalR;

namespace BattleArenaBackendAPI.Hubs
{
    /// <summary>
    /// Global chat hub mapped at /hub/global-chat.
    /// Clients call SendMessage(message); the server broadcasts
    /// ReceiveMessage(username, message) to every connected client.
    /// </summary>
    public class GlobalChatHub : Hub
    {
        public async Task SendMessage(string message)
        {
            // Prefer the authenticated identity; fall back to a generic name for anonymous connections.
            var username = Context.User?.Identity?.Name ?? "Anonymous";

            await Clients.All.SendAsync("ReceiveMessage", username, message);
        }
    }
}
