using System.Collections.Generic;

namespace StarChatBackend.DTOs;

public class ChatMessageWrapper
{
    public List<ChatMessageDto> Messages { get; set; }
}
