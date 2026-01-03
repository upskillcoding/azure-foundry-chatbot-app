using Chatbot.Interfaces;
using Serilog;

namespace Chatbot.Services;

public class CommandHandler : ICommandHandler
{
    private static readonly string[] ExitCommands = { "exit", "quit", "bye", "/exit", "/quit" };
    private static readonly string[] ClearCommands = { "clear", "/clear", "reset", "/reset" };
    private static readonly string[] HelpCommands = { "help", "/help", "?" };
    private static readonly string[] StatsCommands = { "stats", "/stats", "info", "/info" };

    private readonly IConversationService _conversationService;
    private readonly IConsoleService _console;
    private readonly IChatService _chatService;
    private readonly ILogger _logger;

    public CommandHandler(
        IConversationService conversationService,
        IConsoleService console,
        IChatService chatService,
        ILogger logger)
    {
        _conversationService = conversationService;
        _console = console;
        _chatService = chatService;
        _logger = logger;
    }

    public async Task<bool> HandleCommandAsync(string input)
    {
        var lowerInput = input.ToLower().Trim();

        if (ClearCommands.Contains(lowerInput))
        {
            await HandleClearCommandAsync();
            return true;
        }

        if (HelpCommands.Contains(lowerInput))
        {
            ShowHelp();
            return true;
        }

        if (StatsCommands.Contains(lowerInput))
        {
            ShowStats();
            return true;
        }

        return false;
    }

    private async Task HandleClearCommandAsync()
    {
        var messageCount = _conversationService.GetMessageCount();
        _conversationService.ClearHistory();
        
        _console.Clear();
        _console.WriteHeader();
        _console.WriteSuccess($"Conversation history cleared! ({messageCount} messages removed)");
        _console.WriteLine();
        
        _logger.Information("User cleared conversation history");
    }

    private void ShowHelp()
    {
        _console.WriteLine();
        _console.WriteInfo("Available Commands:");
        _console.WriteLine("  help, /help, ?          - Show this help message");
        _console.WriteLine("  clear, /clear, reset    - Clear conversation history");
        _console.WriteLine("  stats, /stats, info     - Show conversation statistics");
        _console.WriteLine("  exit, quit, bye         - Exit the chatbot");
        _console.WriteLine();
    }

    private void ShowStats()
    {
        var messageCount = _conversationService.GetMessageCount();
        var estimatedTokens = _chatService.EstimateTokenCount(_conversationService.MessageHistory);

        _console.WriteLine();
        _console.WriteInfo("Conversation Statistics:");
        _console.WriteLine($"  Messages in history: {messageCount}");
        _console.WriteLine($"  Estimated tokens: ~{estimatedTokens:N0}");
        _console.WriteLine($"  User messages: {_conversationService.MessageHistory.Count(m => m is OpenAI.Chat.UserChatMessage)}");
        _console.WriteLine($"  Assistant messages: {_conversationService.MessageHistory.Count(m => m is OpenAI.Chat.AssistantChatMessage)}");
        _console.WriteLine();
    }

    public bool IsExitCommand(string input)
    {
        return ExitCommands.Contains(input.ToLower().Trim());
    }
}