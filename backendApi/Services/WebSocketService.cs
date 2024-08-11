using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using StarChatBackend.DTOs;
using StarChatBackend.Models;

namespace StarChatBackend.Services;

public class WebSocketService
{
    // this stores all of the WebSockets grouped by a server ID
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocketConnection>> _serverSockets 
        = new ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocketConnection>>();
    
    private readonly ChatMessageService _chatMessageService;
    private readonly UserService _userService;

    public WebSocketService(ChatMessageService chatMessageService, UserService userService)
    {
        _chatMessageService = chatMessageService;
        _userService = userService;
    }
    
    public async Task HandleWebSocketAsync(string serverId, WebSocket webSocket, string userId)
    {
        var serverSockets = _serverSockets.GetOrAdd(serverId, new ConcurrentDictionary<string, WebSocketConnection>());

        var socketId = Guid.NewGuid().ToString();
        var connection = new WebSocketConnection
        {
            SocketId = socketId,
            WebSocket = webSocket,
            UserId = userId
        };
        serverSockets.TryAdd(socketId, connection);

        // optimal size, we don't want bigger messages and we won't read past the buffer size
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result =
            await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            var messageContent = Encoding.UTF8.GetString(buffer, 0, result.Count);
            
            // save the message to the database first
            ChatMessageDto formatedMessage;
            try
            {
                var savedMessage = await _chatMessageService.SendMessageAsync(serverId, userId, messageContent);
                formatedMessage = new ChatMessageDto(savedMessage);
            }
            catch (Exception ex)
            {
                continue;
            }
            
            // then send the message to other active websockets
            await BroadcastMessageAsync(connection, formatedMessage, serverId);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        
        serverSockets.TryRemove(socketId, out _);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    private async Task BroadcastMessageAsync(WebSocketConnection senderConnection, ChatMessageDto message, string serverId)
    {
        var serverSockets = _serverSockets.ContainsKey(serverId) ? _serverSockets[serverId] : null;

        // should never happen, just in case
        if (serverSockets == null) return;

        var jsonMessage = JsonSerializer.Serialize(message);

        foreach (var connection in serverSockets.Values)
        {
            var bytes = Encoding.UTF8.GetBytes(jsonMessage);
            await connection.WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
