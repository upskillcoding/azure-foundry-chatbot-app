using DotNetEnv;

namespace Chatbot.Configuration;

public static class ConfigurationLoader
{
    public static ChatbotConfiguration Load()
    {
        LoadEnvironmentFile();

        var config = new ChatbotConfiguration
        {
            ProjectEndpoint = GetEnvVar<string>("PROJECT_ENDPOINT"),
            ModelDeploymentName = GetEnvVar<string>("MODEL_DEPLOYMENT_NAME"),
            ApiKey = GetEnvVar<string>("API_KEY"),
            MaxOutputTokens = GetEnvVar<int>("MAX_OUTPUT_TOKENS"),
            Temperature = GetEnvVar<float>("TEMPERATURE"),
            TopP = GetEnvVar<float>("TOP_P"),
            SystemPrompt = GetEnvVar<string>("SYSTEM_PROMPT")
        };

        return config;
    }

    private static void LoadEnvironmentFile()
    {
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, ".env");

        if (!File.Exists(dotenv))
        {
            throw new FileNotFoundException(
                $".env file not found at: {dotenv}\n" +
                "Please create a .env file with required configuration values.");
        }

        Env.Load(dotenv);
    }
    private static T GetEnvVar<T>(string name, T? defaultValue = default)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            if (defaultValue != null)
                return defaultValue;

            throw new InvalidOperationException(
                $"Required environment variable '{name}' is not set in .env file.");
        }

        if (typeof(T) == typeof(string))
            return (T)(object)value;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception)
        {
            throw new InvalidOperationException(
                $"Environment variable '{name}' must be a valid {typeof(T).Name}. Got: {value}");
        }
    }
}