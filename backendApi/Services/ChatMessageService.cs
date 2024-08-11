using MongoDB.Driver;
using StarChatBackend.DTOs;
using StarChatBackend.Models;

namespace StarChatBackend.Services;

public class ChatMessageService
{
    private readonly IMongoCollection<ChatMessage> _chatMessagesCollection;
    private readonly ServerService _serverService;
    private readonly UserService _userService;

    public ChatMessageService(MongoDbContext mongoDbContext, ServerService serverService, UserService userService)
    {
        _chatMessagesCollection = mongoDbContext.ChatMessages;
        _serverService = serverService;
        _userService = userService;
    }

    public async Task<ChatMessage> SendMessageAsync(string serverId, string senderId, string content)
    {
        if (string.IsNullOrEmpty(senderId))
        {
            throw new UnauthorizedAccessException("Unauthorized Access");
        }
        
        if (string.IsNullOrEmpty(serverId))
        {
            throw new ArgumentException("Server ID is required.");
        }
        
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentException("Content is required.");
        }

        var server = await _serverService.GetByIdAsync(serverId);
        if (server == null)
        {
            throw new KeyNotFoundException("Server not found.");
        }
        
        if (!server.UserIds.Contains(senderId))
        {
            throw new UnauthorizedAccessException("User is not a member of the server.");
        }

        var senderUsername = await _userService.GetUsernameByIdAsync(senderId);

        var message = new ChatMessage
        {
            ServerId = serverId,
            SenderId = senderId,
            SenderUsername = senderUsername,
            Content = content,
            SentDate = DateTime.UtcNow
        };

        await _chatMessagesCollection.InsertOneAsync(message);
        return message;
    }
    
    public async Task<List<ChatMessageDto>> GetMessagesByServerIdAsync(string serverId, string senderId, int skip = 0, int limit = 50)
    {
        if (string.IsNullOrEmpty(serverId))
        {
            throw new ArgumentException("Server ID is required.");
        }

        var server = await _serverService.GetByIdAsync(serverId);
        if (server == null)
        {
            throw new KeyNotFoundException("Server not found.");
        }
        
        if (!server.UserIds.Contains(senderId))
        {
            throw new UnauthorizedAccessException("User is not a member of the server.");
        }

        var filter = Builders<ChatMessage>.Filter.Eq(m => m.ServerId, serverId);
        var chatMessages = await _chatMessagesCollection.Find(filter)
            .SortBy(m => m.SentDate)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
        
        var chatMessageDtos = chatMessages.Select(message => new ChatMessageDto(message)).ToList();

        return chatMessageDtos;
    }
}
