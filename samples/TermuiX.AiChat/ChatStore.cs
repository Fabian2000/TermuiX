using System.Text.Json;
using System.Text.Json.Serialization;

namespace TermuiX.AiChat;

class ChatData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "New Chat";

    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = [];

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

class ChatStore
{
    private readonly string _chatsDir;

    public ChatStore()
    {
        _chatsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".termui-aichat", "chats");
        Directory.CreateDirectory(_chatsDir);
    }

    public List<ChatData> ListChats()
    {
        var chats = new List<ChatData>();
        if (!Directory.Exists(_chatsDir)) return chats;

        foreach (var file in Directory.GetFiles(_chatsDir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var chat = JsonSerializer.Deserialize(json, StoreJsonCtx.Default.ChatData);
                if (chat is not null)
                    chats.Add(chat);
            }
            catch
            {
                // Skip corrupted files
            }
        }

        return chats.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public ChatData? LoadChat(string id)
    {
        var path = Path.Combine(_chatsDir, $"{id}.json");
        if (!File.Exists(path)) return null;

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize(json, StoreJsonCtx.Default.ChatData);
        }
        catch
        {
            return null;
        }
    }

    public void SaveChat(ChatData chat)
    {
        var path = Path.Combine(_chatsDir, $"{chat.Id}.json");
        var json = JsonSerializer.Serialize(chat, StoreJsonCtx.Default.ChatData);
        File.WriteAllText(path, json);
    }

    public void DeleteChat(string id)
    {
        var path = Path.Combine(_chatsDir, $"{id}.json");
        if (File.Exists(path))
            File.Delete(path);
    }

    public void RenameChat(string id, string newTitle)
    {
        var chat = LoadChat(id);
        if (chat is null) return;
        chat.Title = newTitle;
        SaveChat(chat);
    }

    public ChatData CreateChat(string model = "")
    {
        var chat = new ChatData
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Title = "New Chat",
            Model = model,
            CreatedAt = DateTime.UtcNow
        };
        SaveChat(chat);
        return chat;
    }
}

[JsonSerializable(typeof(ChatData))]
[JsonSerializable(typeof(List<ChatData>))]
partial class StoreJsonCtx : JsonSerializerContext { }
