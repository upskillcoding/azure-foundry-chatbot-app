namespace Chatbot.Interfaces;

public interface IConsoleService
{
    void WriteHeader();
    void WriteInfo(string message);
    void WriteSuccess(string message);
    void WriteError(string message);
    void WriteUserPrompt(string timestamp);
    void WriteAssistantPrompt(string timestamp);
    void Write(string text);
    void WriteLine(string text = "");
    string? ReadLine();
    void Clear();
}
