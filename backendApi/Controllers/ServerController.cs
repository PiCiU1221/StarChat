using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarChatBackend.DTOs;
using StarChatBackend.Services;

namespace StarChatBackend.Controllers;

[ApiController]
[Route("api/servers")]
public class ServerController : ControllerBase
{
    private readonly ServerService _serverService;

    public ServerController(ServerService serverService)
    {
        _serverService = serverService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateServer([FromBody] CreateServerRequestDto requestDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }
        
        try
        {
            var newServer = await _serverService.CreateServerAsync(requestDto.ServerName, userId);
            return StatusCode(201, newServer);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
    
    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetServer(string id)
    {
        var server = await _serverService.GetByIdAsync(id);
        if (server == null)
        {
            return NotFound();
        }
        return Ok(server);
    }

    [HttpPost("{serverId}/members")]
    [Authorize]
    public async Task<IActionResult> JoinServer(string serverId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }
        
        try
        {
            await _serverService.AddUserToServerAsync(serverId, userId);
            return Ok(new { message = "Successfully joined the server." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{serverId}/members")]
    [Authorize]
    public async Task<IActionResult> LeaveServer(string serverId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }
        
        try
        {
            await _serverService.RemoveUserFromServerAsync(serverId, userId);
            return Ok(new { message = "Successfully left the server." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("ids-to-names")]
    [Authorize]
    public async Task<IActionResult> GetServerNamesByIds([FromQuery] List<string> serverIds)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }

        if (serverIds == null || !serverIds.Any())
        {
            return BadRequest(new { message = "No server IDs provided." });
        }
        
        try
        {
            var servers = await _serverService.GetServerNamesByIds(serverIds);
            return Ok(new { servers = servers });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
