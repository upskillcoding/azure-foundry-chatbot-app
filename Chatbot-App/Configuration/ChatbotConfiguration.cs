namespace Chatbot.Configuration;

public class ChatbotConfiguration
{
    public string ProjectEndpoint { get; set; } = string.Empty;
    public string ModelDeploymentName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int MaxOutputTokens { get; set; } = 4096;
    public float Temperature { get; set; } = 1.0f;
    public float TopP { get; set; } = 1.0f;
    public int MaxHistoryMessages { get; set; } = 50;
    public string LoggingLevel { get; set; } = "Error";
    public string SystemPrompt { get; set; } = "You are a helpful assistant. Be concise but thorough in your responses.";
}