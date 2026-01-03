using System.ClientModel;
using System.Runtime.CompilerServices;
using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Chatbot.Configuration;
using Chatbot.Interfaces;
using Microsoft.ML.Tokenizers;
using OpenAI.Chat;
using Serilog;

namespace Chatbot.Services;

public class ChatService : IChatService
{
    private readonly ChatClient _chatClient;
    private readonly ChatCompletionOptions _requestOptions;
    private readonly ILogger _logger;
    private readonly Tokenizer _tokenizer;

    public ChatService(ChatbotConfiguration config, ILogger logger)
    {
        _logger = logger;
        _chatClient = InitializeChatClient(config);
        _requestOptions = CreateRequestOptions(config);
        _tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
        _logger.Information("ChatService initialized successfully");
    }

    private ChatClient InitializeChatClient(ChatbotConfiguration config)
    {
        try
        {
            _logger.Information("Initializing Azure OpenAI client...");

            var credential = new DefaultAzureCredential();
            var projectClient = new AIProjectClient(new Uri(config.ProjectEndpoint), credential);
            var connection = projectClient.GetConnection(typeof(AzureOpenAIClient).FullName!);

            if (!connection.TryGetLocatorAsUri(out var connectionUri) || connectionUri is null)
            {
                throw new InvalidOperationException("Failed to retrieve valid URI from connection");
            }

            // Ensure we use HTTPS and proper host
            var azureOpenAiUri = new UriBuilder(connectionUri)
            {
                Scheme = Uri.UriSchemeHttps,
                Port = -1 // Use default port for HTTPS
            }.Uri;

            _logger.Information("Connecting to Azure OpenAI at {Uri}", azureOpenAiUri);

            var azureOpenAIClient = new AzureOpenAIClient(
                azureOpenAiUri,
                new ApiKeyCredential(config.ApiKey));

            return azureOpenAIClient.GetChatClient(config.ModelDeploymentName);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize ChatClient");
            throw new InvalidOperationException(
                "Failed to initialize Azure OpenAI client. Check your configuration and credentials.", ex);
        }
    }

    private static ChatCompletionOptions CreateRequestOptions(ChatbotConfiguration config)
    {
        return new ChatCompletionOptions
        {
            MaxOutputTokenCount = config.MaxOutputTokens,
            Temperature = config.Temperature,
            TopP = config.TopP,
        };
    }

    public async Task<IAsyncEnumerable<string>> SendMessageStreamingAsync(
        List<ChatMessage> messageHistory,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug(
            "Sending message with {MessageCount} messages in history",
            messageHistory.Count);

        IAsyncEnumerable<StreamingChatCompletionUpdate> response;

        try
        {
            response = _chatClient.CompleteChatStreamingAsync(
                messageHistory,
                _requestOptions);
        }
        catch (ClientResultException ex)
        {
            _logger.Error(ex, "API error: Status {Status}", ex.Status);
            throw new InvalidOperationException(GetErrorMessage(ex), ex);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during chat completion");
            throw;
        }

        return StreamResponseAsync(response, cancellationToken);
    }

    private async IAsyncEnumerable<string> StreamResponseAsync(
        IAsyncEnumerable<StreamingChatCompletionUpdate> response,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var update in response.WithCancellation(cancellationToken))
        {
            foreach (var contentPart in update.ContentUpdate)
            {
                if (!string.IsNullOrWhiteSpace(contentPart.Text))
                {
                    yield return contentPart.Text;
                }
            }
        }
    }


    private static string GetErrorMessage(ClientResultException ex)
    {
        return ex.Status switch
        {
            401 => "Authentication failed. Check your API key and credentials.",
            403 => "Access forbidden. Verify your permissions and subscription status.",
            404 => "Model deployment not found. Check your MODEL_DEPLOYMENT_NAME configuration.",
            429 => "Rate limit exceeded. Please wait before sending more messages.",
            500 or 502 or 503 or 504 => "Azure OpenAI service is temporarily unavailable. Please try again later.",
            _ => $"API error (Status {ex.Status}): {ex.Message}"
        };
    }

    public int EstimateTokenCount(List<ChatMessage> messages)
    {
        try
        {
            int totalTokens = 0;

            foreach (var message in messages)
            {
                // Add tokens for message formatting (role, content markers, etc.)
                // Each message has overhead: ~4 tokens for role and formatting
                totalTokens += 4;

                // Get message content and count tokens
                string content = message switch
                {
                    UserChatMessage user => string.Join("", user.Content.Select(c => c.Text ?? "")),
                    AssistantChatMessage assistant => string.Join("", assistant.Content.Select(c => c.Text ?? "")),
                    SystemChatMessage system => string.Join("", system.Content.Select(c => c.Text ?? "")),
                    _ => ""
                };

                if (!string.IsNullOrEmpty(content))
                {
                    // Count tokens using Microsoft.ML.Tokenizers
                    int tokenCount = _tokenizer.CountTokens(content);
                    totalTokens += tokenCount;
                }
            }

            // Add 3 tokens for reply priming
            totalTokens += 3;

            _logger.Debug("Estimated token count: {TokenCount} for {MessageCount} messages",
                totalTokens, messages.Count);

            return totalTokens;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to calculate exact token count, using fallback estimation");

            // Fallback to character-based estimation if tokenizer fails
            int totalChars = messages.Sum(m =>
            {
                if (m is UserChatMessage user)
                    return user.Content.Sum(c => c.Text?.Length ?? 0);
                if (m is AssistantChatMessage assistant)
                    return assistant.Content.Sum(c => c.Text?.Length ?? 0);
                if (m is SystemChatMessage system)
                    return system.Content.Sum(c => c.Text?.Length ?? 0);
                return 0;
            });

            return totalChars / 4; // Rough approximation: 4 chars per token
        }
    }
}