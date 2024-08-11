using StarChatBackend.Models;

namespace StarChatBackend.DTOs;

public class UserResponseDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public List<string> JoinedServerIds { get; set; }

    public UserResponseDto(User user)
    {
        Id = user.Id;
        Email = user.Email;
        Username = user.Username;
        JoinedServerIds = user.JoinedServerIds;
    }
}
