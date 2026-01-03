using Chatbot.Interfaces;

namespace Chatbot.Services;

public class ConsoleService : IConsoleService
{
    private readonly object _lock = new();

    public void WriteHeader()
    {
        lock (_lock)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("╔═══════════════════════════════════════╗");
            Console.WriteLine("║   Azure Foundry OpenAI Chatbot v2.0   ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    public void WriteInfo(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }
    }

    public void WriteSuccess(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }
    }

    public void WriteError(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }
    }

    public void WriteUserPrompt(string timestamp)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{timestamp}] You     : ");
            Console.ResetColor();
        }
    }

    public void WriteAssistantPrompt(string timestamp)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"[{timestamp}] Chatbot : ");
            Console.ResetColor();
        }
    }

    public void Write(string text)
    {
        lock (_lock)
        {
            Console.Write(text);
        }
    }

    public void WriteLine(string text = "")
    {
        lock (_lock)
        {
            Console.WriteLine(text);
        }
    }

    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public void Clear()
    {
        lock (_lock)
        {
            Console.Clear();
        }
    }
}