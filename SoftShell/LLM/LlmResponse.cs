using System.Text.Json.Serialization;

namespace SoftShell.LLM;

public record LlmResponse(
    [property: JsonPropertyName("Command")] string Command,
    [property: JsonPropertyName("Explanation")] string Explanation
);

public record ChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

public record ChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
    [property: JsonPropertyName("temperature")] float Temperature = 0.5f
);

public record ChatChoice(
    [property: JsonPropertyName("message")] ChatMessage Message
);

public record ChatResponse(
    [property: JsonPropertyName("choices")] List<ChatChoice> Choices
);

[JsonSerializable(typeof(LlmResponse))]
[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatResponse))]
public partial class SoftShellJsonContext : JsonSerializerContext
{
}
