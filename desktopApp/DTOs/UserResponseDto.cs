using System.Collections.Generic;

namespace StarChatBackend.DTOs;

public class UserResponseDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public List<string> JoinedServerIds { get; set; }
}
