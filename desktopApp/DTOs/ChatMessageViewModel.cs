namespace StarChatBackend.DTOs;

public class ChatMessageViewModel
{
    public string Id { get; set; }
    public string SenderId { get; set; }
    public string SenderUsername { get; set; }
    public string Content { get; set; }
    public string SendDate { get; set; }
    public bool IsUserMessage { get; set; }

    public ChatMessageViewModel(ChatMessageDto chatMessageDto, bool isUserMessage)
    {
        Id = chatMessageDto.Id;
        SenderId = chatMessageDto.SenderId;
        SenderUsername = chatMessageDto.SenderUsername;
        Content = chatMessageDto.Content;
        SendDate = chatMessageDto.SendDate;
        IsUserMessage = isUserMessage;
    }
}
