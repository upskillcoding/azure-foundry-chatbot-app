using Chatbot.Configuration;
using Chatbot.Interfaces;
using OpenAI.Chat;
using Serilog;

namespace Chatbot.Services;

public class ConversationService : IConversationService
{
    private readonly List<ChatMessage> _messageHistory;
    private readonly ChatbotConfiguration _config;
    private readonly ILogger _logger;
    public List<ChatMessage> MessageHistory => _messageHistory;

    public ConversationService(ChatbotConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
        _messageHistory = new List<ChatMessage>
        {
            new SystemChatMessage(config.SystemPrompt)
        };

        _logger.Information("ConversationService initialized with system prompt");
    }

    public void AddUserMessage(string message)
    {
        _messageHistory.Add(new UserChatMessage(message));
        _logger.Debug("User message added. History count: {Count}", _messageHistory.Count);
    }

    public void AddAssistantMessage(string message)
    {
        _messageHistory.Add(new AssistantChatMessage(message));
        _logger.Debug("Assistant message added. History count: {Count}", _messageHistory.Count);
    }

    public void ClearHistory()
    {
        var systemMessage = _messageHistory.FirstOrDefault(m => m is SystemChatMessage);
        var clearedCount = _messageHistory.Count - 1;
        
        _messageHistory.Clear();
        
        if (systemMessage != null)
        {
            _messageHistory.Add(systemMessage);
        }

        _logger.Information("Conversation history cleared. {Count} messages removed", clearedCount);
    }

    // Exclude system message from count
    public int GetMessageCount() => _messageHistory.Count(m => m is not SystemChatMessage);
    

    public void TrimHistory(int maxMessages)
    {
        if (_messageHistory.Count <= maxMessages + 1) // +1 for system message
            return;

        var systemMessage = _messageHistory.FirstOrDefault(m => m is SystemChatMessage);
        var conversationMessages = _messageHistory.Where(m => m is not SystemChatMessage).ToList();

        // Keep most recent messages
        var messagesToKeep = conversationMessages.TakeLast(maxMessages).ToList();
        var removedCount = conversationMessages.Count - messagesToKeep.Count;

        _messageHistory.Clear();
        
        if (systemMessage != null)
        {
            _messageHistory.Add(systemMessage);
        }
        
        _messageHistory.AddRange(messagesToKeep);

        _logger.Information("History trimmed. Removed {RemovedCount} old messages", removedCount);
    }

}