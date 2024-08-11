using System.Net.WebSockets;

namespace StarChatBackend.Models;

public class WebSocketConnection
{
    public string SocketId { get; set; }
    public WebSocket WebSocket { get; set; }
    public string UserId { get; set; }
}
