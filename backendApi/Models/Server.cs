using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StarChatBackend.Models;

public class Server
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("ownerId")]
    public string OwnerId { get; set; } = string.Empty;

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [BsonElement("userIds")] public List<string> UserIds { get; set; } = new List<string>();
}
