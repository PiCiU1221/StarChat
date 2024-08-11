namespace StarChatBackend.DTOs;

public class ChatMessageDto
{
    public string Id { get; set; }
    public string SenderId { get; set; }
    public string SenderUsername { get; set; }
    public string Content { get; set; }
    public string SendDate { get; set; }
}
