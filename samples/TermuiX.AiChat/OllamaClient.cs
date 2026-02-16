using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TermuiX.AiChat;

public record ChatMessage(string Role, string Content, DateTime? Timestamp = null);
public record StreamToken(string Text, bool IsThinking);

public class OllamaClient : IDisposable
{
    private readonly HttpClient _http;
    private CancellationTokenSource? _currentStream;

    public OllamaClient(string baseUrl = "http://localhost:11434")
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(10)
        };
    }

    public async Task<List<string>> ListModelsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync("/api/tags", JsonCtx.Default.TagsResponse);
            if (response?.Models is null) return [];
            return response.Models.Select(m => m.Name).OrderBy(n => n).ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _http.GetAsync("/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async IAsyncEnumerable<StreamToken> ChatStreamAsync(
        string model,
        List<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Cancel any previous stream
        _currentStream?.Cancel();
        _currentStream = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _currentStream.Token;

        var request = new ChatRequest
        {
            Model = model,
            Messages = messages.Select(m => new ApiMessage { Role = m.Role, Content = m.Content }).ToList(),
            Stream = true,
            KeepAlive = "1m"
        };

        var json = JsonSerializer.Serialize(request, JsonCtx.Default.ChatRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        string? connectionError = null;
        try
        {
            response = await _http.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, "/api/chat") { Content = content },
                HttpCompletionOption.ResponseHeadersRead,
                token);
            response.EnsureSuccessStatusCode();
        }
        catch (OperationCanceledException)
        {
            yield break;
        }
        catch (HttpRequestException)
        {
            connectionError = "[Error: Could not connect to Ollama. Is it running?]";
            response = null!;
        }

        if (connectionError is not null)
        {
            yield return new StreamToken(connectionError, false);
            yield break;
        }

        using (response)
        await using (var stream = await response.Content.ReadAsStreamAsync(token))
        using (var reader = new StreamReader(stream))
        {
            while (!token.IsCancellationRequested)
            {
                string? line;
                try
                {
                    line = await reader.ReadLineAsync(token);
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }

                if (line is null) yield break;
                if (line.Length == 0) continue;

                ChatStreamResponse? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize(line, JsonCtx.Default.ChatStreamResponse);
                }
                catch
                {
                    continue;
                }

                if (chunk is null) continue;

                if (chunk.Message?.Thinking is { Length: > 0 } thinkText)
                    yield return new StreamToken(thinkText, true);

                if (chunk.Message?.Content is { Length: > 0 } tokenText)
                    yield return new StreamToken(tokenText, false);

                if (chunk.Done)
                    yield break;
            }
        }
    }

    public void CancelStream()
    {
        _currentStream?.Cancel();
    }

    public async Task UnloadModelAsync(string model)
    {
        try
        {
            var request = new ChatRequest
            {
                Model = model,
                Messages = [],
                Stream = false,
                KeepAlive = "0"
            };
            var json = JsonSerializer.Serialize(request, JsonCtx.Default.ChatRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _http.PostAsync("/api/chat", content);
        }
        catch
        {
            // Ignore unload errors
        }
    }

    public void Dispose()
    {
        _currentStream?.Cancel();
        _currentStream?.Dispose();
        _http.Dispose();
    }
}

// JSON models for Ollama API

class TagsResponse
{
    [JsonPropertyName("models")]
    public List<ModelInfo>? Models { get; set; }
}

class ModelInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

class ChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("messages")]
    public List<ApiMessage> Messages { get; set; } = [];

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;

    [JsonPropertyName("keep_alive")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeepAlive { get; set; }
}

class ApiMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("thinking")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Thinking { get; set; }
}

class ChatStreamResponse
{
    [JsonPropertyName("message")]
    public ApiMessage? Message { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatStreamResponse))]
[JsonSerializable(typeof(TagsResponse))]
partial class JsonCtx : JsonSerializerContext { }
