using StarChatBackend.Services;

namespace StarChatBackend.Tests;

public class MongoDbConnectionTest
{
    private readonly MongoDbContext _mongoDbContext;

    public MongoDbConnectionTest(MongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }

    public async Task CheckDatabaseConnectionAsync()
    {
        bool isConnected = await _mongoDbContext.TestConnectionAsync();
        if (isConnected)
        {
            Console.WriteLine("Successfully connected to the database.");
        }
        else
        {
            Console.WriteLine("Failed to connect to the database.");
        }
    }
}
