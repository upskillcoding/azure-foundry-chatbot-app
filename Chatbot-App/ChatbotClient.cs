using System.Text;
using Chatbot.Configuration;
using Chatbot.Interfaces;
using Serilog;

namespace Chatbot;

public class ChatbotClient
{
    private readonly IChatService _chatService;
    private readonly IConversationService _conversationService;
    private readonly IConsoleService _console;
    private readonly ICommandHandler _commandHandler;
    private readonly ChatbotConfiguration _config;
    private readonly ILogger _logger;

    public ChatbotClient(
        IChatService chatService,
        IConversationService conversationService,
        IConsoleService console,
        ICommandHandler commandHandler,
        ChatbotConfiguration config,
        ILogger logger)
    {
        _chatService = chatService;
        _conversationService = conversationService;
        _console = console;
        _commandHandler = commandHandler;
        _config = config;
        _logger = logger;

        _console.WriteHeader();
    }

    public async Task RunChatLoopAsync()
    {
        _console.WriteInfo("Type 'help' for commands or start chatting!");
        _console.WriteLine();

        _logger.Information("Chat loop started");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            _logger.Information("Cancellation requested by user");
        };

        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                var userInput = await GetUserInputAsync();

                if (string.IsNullOrWhiteSpace(userInput))
                    continue;

                // Handle commands
                if (await _commandHandler.HandleCommandAsync(userInput))
                    continue;

                // Check for exit
                if (_commandHandler.IsExitCommand(userInput))
                {
                    _console.WriteInfo("Goodbye! 👋");
                    _logger.Information("User exited chat");
                    break;
                }

                // Process chat message
                await ProcessChatMessageAsync(userInput, cts.Token);

                // Trim history if needed
                _conversationService.TrimHistory(_config.MaxHistoryMessages);
            }
            catch (OperationCanceledException)
            {
                _console.WriteLine();
                _console.WriteInfo("Operation cancelled by user");
                _logger.Information("Operation cancelled");
                break;
            }
            catch (Exception ex)
            {
                _console.WriteError($"Error: {ex.Message}");
                _console.WriteLine();
                _logger.Error(ex, "Error in chat loop");
            }
        }
    }

    private async Task<string> GetUserInputAsync()
    {
        var timestamp = DateTime.Now.ToString("HH:mm");
        _console.WriteUserPrompt(timestamp);

        return await Task.Run(() => _console.ReadLine()?.Trim() ?? string.Empty);
    }

    private async Task ProcessChatMessageAsync(string userInput, CancellationToken cancellationToken)
    {
        // Add user message
        _conversationService.AddUserMessage(userInput);

        // Get assistant response
        var timestamp = DateTime.Now.ToString("HH:mm");
        _console.WriteAssistantPrompt(timestamp);

        var response = new StringBuilder();
        var firstTokenReceived = false;

        try
        {
            var streamingResponse = await _chatService.SendMessageStreamingAsync(
                _conversationService.MessageHistory,
                cancellationToken);

            await foreach (var token in streamingResponse.WithCancellation(cancellationToken))
            {
                if (!firstTokenReceived)
                {
                    firstTokenReceived = true;
                    _logger.Debug("First token received");
                }

                _console.Write(token);
                response.Append(token);
            }

            _console.WriteLine("\n");

            // Add assistant response to history
            _conversationService.AddAssistantMessage(response.ToString());
            
            _logger.Debug("Message processing completed. Response length: {Length}", response.Length);
        }
        catch (OperationCanceledException)
        {
            _console.WriteLine();
            _console.WriteInfo("Response cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _console.WriteLine();
            _console.WriteError($"Failed to get response: {ex.Message}");
            
            // Remove the user message since we couldn't process it
            if (_conversationService.MessageHistory.Count > 0)
            {
                var lastMessage = _conversationService.MessageHistory.Last();
                if (lastMessage is OpenAI.Chat.UserChatMessage)
                {
                    _conversationService.MessageHistory.Remove(lastMessage);
                }
            }
            
            throw;
        }
    }
}