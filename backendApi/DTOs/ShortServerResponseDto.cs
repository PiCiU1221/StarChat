namespace StarChatBackend.DTOs;

public class ShortServerResponseDto
{
    public string Id { get; set; }
    public string Name { get; set; }

    public ShortServerResponseDto(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
