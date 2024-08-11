using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using StarChatBackend.Configuration;
using StarChatBackend.Models;

namespace StarChatBackend.Services;

public class MongoDbContext
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        _mongoClient = new MongoClient(options.Value.ConnectionString);
        _database = _mongoClient.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoClient MongoClient => _mongoClient;
    
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Server> Servers => _database.GetCollection<Server>("servers");
    public IMongoCollection<ChatMessage> ChatMessages => _database.GetCollection<ChatMessage>("chatMessages");
    
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var command = new BsonDocument("ping", 1);
            var result = await _database.RunCommandAsync<BsonDocument>(command);
            return result != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database connection error: {ex.Message}");
            return false;
        }
    }
}
