namespace Chatbot.Interfaces;

public interface ICommandHandler
{
    Task<bool> HandleCommandAsync(string input);
    bool IsExitCommand(string input);
}