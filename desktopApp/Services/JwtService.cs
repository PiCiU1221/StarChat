using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Windows;

namespace StarChatDesktopApp.Services;

public class JwtService
{
    public static string? GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        if (jsonToken == null)
        {
            return null;
        }

        var userIdClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "nameid");
        
        return userIdClaim?.Value;
    }
}
