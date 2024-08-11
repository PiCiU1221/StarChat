using StarChatBackend.Models;
using MongoDB.Driver;
using StarChatBackend.DTOs;

namespace StarChatBackend.Services;

public class UserService
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserService(MongoDbContext context)
    {
        _usersCollection = context.Users;
    }
    
    public async Task<UserResponseDto?> GetByIdAsync(string id)
    {
        var user = await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return new UserResponseDto(user);
    }

    
    public async Task<User?> GetByEmailAsync(string email) =>
        await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();
    
    public async Task CreateAsync(User newUser) =>
        await _usersCollection.InsertOneAsync(newUser);
    
    public async Task UpdateUsernameAsync(string userId, string username)
    {
        var update = Builders<User>.Update.Set(u => u.Username, username);
        var result = await _usersCollection.UpdateOneAsync(u => u.Id == userId, update);

        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (result.ModifiedCount == 0)
        {
            throw new InvalidOperationException("Failed to update username.");
        }
    }

    public async Task AddServerToUserAsync(IClientSessionHandle session, string userId, string serverId)
    {
        var update = Builders<User>.Update.AddToSet(u => u.JoinedServerIds, serverId);
        var result = await _usersCollection.UpdateOneAsync(session, u => u.Id == userId, update);

        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (result.ModifiedCount == 0)
        {
            throw new InvalidOperationException("Failed to join server.");
        }
    }
    
    public async Task RemoveServerFromUserAsync(IClientSessionHandle session, string userId, string serverId)
    {
        var update = Builders<User>.Update.Pull(u => u.JoinedServerIds, serverId);
        var result = await _usersCollection.UpdateOneAsync(session, u => u.Id == userId, update);

        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (result.ModifiedCount == 0)
        {
            throw new InvalidOperationException("Failed to leave server.");
        }
    }
    
    public async Task<string> GetUsernameByIdAsync(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var projection = Builders<User>.Projection.Expression(u => u.Username);
        var username = await _usersCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

        return username ?? string.Empty;
    }
}
