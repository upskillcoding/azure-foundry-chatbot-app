
using Chatbot;
using Chatbot.Configuration;
using Chatbot.Interfaces;
using Chatbot.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

class Program
{
    static async Task Main(string[] args)
    {
        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine("logs", "chatbot-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        try
        {
            Log.Information("Application starting...");

            // Load configuration
            var config = ConfigurationLoader.Load();
            Log.Information("Configuration loaded successfully");

            // Setup dependency injection
            var serviceProvider = ConfigureServices(config);

            // Run the chatbot
            var chatbot = serviceProvider.GetRequiredService<ChatbotClient>();
            await chatbot.RunChatLoopAsync();

            Log.Information("Application shutting down normally");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unexpected fatal error");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ Fatal Error: {ex.Message}");
            Console.WriteLine("\nCheck the logs for more details.");
            Console.ResetColor();
            Environment.Exit(1);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IServiceProvider ConfigureServices(ChatbotConfiguration config)
    {
        var services = new ServiceCollection();

        // Register configuration
        services.AddSingleton(config);

        // Register logger
        services.AddSingleton<ILogger>(Log.Logger);

        // Register services
        services.AddSingleton<IConsoleService, ConsoleService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IConversationService, ConversationService>();
        services.AddSingleton<ICommandHandler>(sp => new CommandHandler(
            sp.GetRequiredService<IConversationService>(),
            sp.GetRequiredService<IConsoleService>(),
            sp.GetRequiredService<IChatService>(),
            sp.GetRequiredService<ILogger>()
        ));

        // Register main client
        services.AddSingleton<ChatbotClient>();

        return services.BuildServiceProvider();
    }
}