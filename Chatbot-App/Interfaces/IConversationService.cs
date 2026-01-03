using OpenAI.Chat;

namespace Chatbot.Interfaces;

public interface IConversationService
{
    List<ChatMessage> MessageHistory { get; }
    void AddUserMessage(string message);
    void AddAssistantMessage(string message);
    void ClearHistory();
    int GetMessageCount();
    void TrimHistory(int maxMessages);
}