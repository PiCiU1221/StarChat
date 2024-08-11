using MongoDB.Driver;
using StarChatBackend.DTOs;
using StarChatBackend.Models;

namespace StarChatBackend.Services;

public class AuthService
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;

    public AuthService(UserService userService, TokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Passwords do not match.");
        }

        var existingUser = await _userService.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ArgumentException("Email already taken.");
        }

        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userService.CreateAsync(newUser);

        return new RegisterResponseDto(newUser);
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await _userService.GetByEmailAsync(email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        
        var passwordsMatch = BCrypt.Net.BCrypt.Verify(password, user.Password);

        if (!passwordsMatch)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        
        return _tokenService.GenerateToken(user);
    }
}
