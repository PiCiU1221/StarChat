using StarChatBackend.Models;

namespace StarChatBackend.DTOs;

public class ChatMessageDto
{
    public string Id { get; set; }
    public string SenderId { get; set; }
    public string SenderUsername { get; set; }
    public string Content { get; set; }
    public string SendDate { get; set; }

    public ChatMessageDto(ChatMessage chatMessage)
    {
        Id = chatMessage.Id.ToString();
        SenderId = chatMessage.SenderId;
        SenderUsername = chatMessage.SenderUsername;
        Content = chatMessage.Content;
        SendDate = chatMessage.SentDate.ToString("dd.MM.yyyy HH:mm");
    }
}
