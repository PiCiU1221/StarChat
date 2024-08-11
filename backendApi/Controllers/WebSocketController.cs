using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarChatBackend.Services;

namespace StarChatBackend.Controllers;

[ApiController]
[Route("api/servers/{serverId}")]
public class WebSocketController : ControllerBase
{
    private readonly WebSocketService _webSocketService;
    private readonly TokenService _tokenService;

    public WebSocketController(WebSocketService webSocketService, TokenService tokenService)
    {
        _webSocketService = webSocketService;
        _tokenService = tokenService;
    }

    [HttpGet("connect")]
    public async Task<IActionResult> Connect([FromRoute] string serverId, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Token is required.");
        }
        
        var userId = _tokenService.ValidateTokenAndExtractUserId(token);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Invalid or expired token." });
        }
        
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            return BadRequest("WebSocket request required.");
        }

        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        await _webSocketService.HandleWebSocketAsync(serverId, webSocket, userId);

        return new EmptyResult();
    }
}
