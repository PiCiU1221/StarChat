using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StarChatBackend.Models;

public class ChatMessage
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("serverId")]
    public string ServerId { get; set; } = string.Empty;
    
    [BsonElement("senderId")]
    public string SenderId { get; set; } = string.Empty;

    [BsonElement("senderUsername")]
    public string SenderUsername { get; set; } = string.Empty;
    
    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;
    
    [BsonElement("sentDate")]
    public DateTime SentDate { get; set; }
}
