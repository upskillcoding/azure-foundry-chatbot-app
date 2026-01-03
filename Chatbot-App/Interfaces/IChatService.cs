using OpenAI.Chat;

namespace Chatbot.Interfaces;

public interface IChatService
{
    Task<IAsyncEnumerable<string>> SendMessageStreamingAsync(
        List<ChatMessage> messageHistory,
        CancellationToken cancellationToken = default);
    
    int EstimateTokenCount(List<ChatMessage> messages);
}