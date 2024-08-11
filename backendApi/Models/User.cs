using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StarChatBackend.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("password")]
    public string Password { get; set; } = string.Empty;

    [BsonElement("joinedServerIds")] public List<string> JoinedServerIds { get; set; } = new List<string>();
}
