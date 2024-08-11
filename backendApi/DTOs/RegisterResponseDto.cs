using StarChatBackend.Models;

namespace StarChatBackend.DTOs;

public class RegisterResponseDto
{
    public string Email { get; set; }
    public string Username { get; set; }

    public RegisterResponseDto(User user)
    {
        Email = user.Email;
        Username = user.Username;
    }
}
