using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarChatBackend.Services;

namespace StarChatBackend.Controllers;

[ApiController]
[Route("api/servers/{serverId}/messages")]
public class ChatMessageController : ControllerBase
{
    private readonly ChatMessageService _chatMessageService;

    public ChatMessageController(ChatMessageService chatMessageService)
    {
        _chatMessageService = chatMessageService;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMessages(string serverId, int page = 1, int pageSize = 50)
    {
        var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(senderId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }
        
        try
        {
            int skip = (page - 1) * pageSize;

            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "Page and pageSize must be positive numbers." });
            }

            if (pageSize > 50)
            {
                return BadRequest(new { message = "Page must be smaller or equal to 50." });
            }

            var messages = await _chatMessageService.GetMessagesByServerIdAsync(serverId, senderId, skip, pageSize);
            return Ok(new { messages = messages });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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
}
