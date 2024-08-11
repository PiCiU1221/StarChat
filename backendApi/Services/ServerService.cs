using MongoDB.Driver;
using StarChatBackend.DTOs;
using StarChatBackend.Models;

namespace StarChatBackend.Services;

public class ServerService
{
    private readonly IMongoCollection<Server> _serversCollection;
    private readonly UserService _userService;
    private readonly MongoDbContext _mongoDbContext;

    public ServerService(MongoDbContext mongoDbContext, UserService userService)
    {
        _serversCollection = mongoDbContext.Servers;
        _userService = userService;
        _mongoDbContext = mongoDbContext;
    }

    public async Task<Server> CreateServerAsync(string name, string ownerId)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Server name is required.");
        }

        var owner = await _userService.GetByIdAsync(ownerId);
        if (owner == null)
        {
            throw new ArgumentException("Owner not found.");
        }

        var newServer = new Server
        {
            Name = name,
            OwnerId = ownerId,
            CreatedDate = DateTime.UtcNow,
            UserIds = new List<string> { ownerId }
        };

        using (var session = await _mongoDbContext.MongoClient.StartSessionAsync())
        {
            session.StartTransaction();

            try
            {
                await _serversCollection.InsertOneAsync(session, newServer);

                await _userService.AddServerToUserAsync(session, ownerId, newServer.Id);

                await session.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        return newServer;
    }

    public async Task<Server?> GetByIdAsync(string id) =>
        await _serversCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task AddUserToServerAsync(string serverId, string userId)
    {
        using (var session = await _mongoDbContext.MongoClient.StartSessionAsync())
        {
            session.StartTransaction();

            try
            {
                var updateServer = Builders<Server>.Update.AddToSet(s => s.UserIds, userId);
                var resultServer = await _serversCollection.UpdateOneAsync(session, s => s.Id == serverId, updateServer);

                if (resultServer.MatchedCount == 0)
                {
                    throw new KeyNotFoundException("Server not found.");
                }
                
                if (resultServer.ModifiedCount == 0)
                {
                    throw new InvalidOperationException("Failed to add user to the server.");
                }

                await _userService.AddServerToUserAsync(session, userId, serverId);

                await session.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }

    public async Task RemoveUserFromServerAsync(string serverId, string userId)
    {
        using (var session = await _mongoDbContext.MongoClient.StartSessionAsync())
        {
            session.StartTransaction();

            try
            {
                var updateServer = Builders<Server>.Update.Pull(s => s.UserIds, userId);
                var resultServer = await _serversCollection.UpdateOneAsync(session, s => s.Id == serverId, updateServer);

                if (resultServer.MatchedCount == 0)
                {
                    throw new KeyNotFoundException("Server not found.");
                }

                if (resultServer.ModifiedCount == 0)
                {
                    throw new InvalidOperationException("Failed to remove user from the server.");
                }

                await _userService.RemoveServerFromUserAsync(session, userId, serverId);

                await session.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }

    public async Task<List<ShortServerResponseDto>> GetServerNamesByIds(List<string> serverIds)
    {
        var filter = Builders<Server>.Filter.In(s => s.Id, serverIds);
        var projection = Builders<Server>.Projection.Expression(s => new ShortServerResponseDto(s.Id, s.Name));

        var servers = await _serversCollection.Find(filter).Project(projection).ToListAsync();
        return servers;
    }
}
