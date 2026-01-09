# Azure Foundry OpenAI Chatbot v2.0

A .NET 9.0 console application that provides an interactive chat interface with Azure OpenAI models deployed through Azure AI Foundry.

<img width="1122" height="976" alt="Screenshot from 2026-01-03 22-33-35" src="https://github.com/user-attachments/assets/6538be10-4ac1-4c96-ba74-7b6e8ca1dc90" />


## Features

### Core Functionality
- **Streaming Responses**: Real-time token streaming for immediate feedback
- **Conversation History**: Automatic message history management with configurable limits
- **Persistent Logging**: Structured logging with Serilog to console and rotating files
- **Error Handling**: Comprehensive error handling with user-friendly messages
  
### Commands
- `help`, `/help`, `?` - Display available commands
- `clear`, `/clear`, `reset` - Clear conversation history
- `stats`, `/stats`, `info` - Show conversation statistics
- `exit`, `quit`, `bye` - Exit the application

### Advanced Features
- **Token Estimation**: Approximate token counting for context management
- **Conversation Export**: Save conversations as JSON with timestamps
- **Configuration Validation**: Comprehensive validation of all settings
- **Dependency Injection**: Clean architecture with proper DI patterns
- **Cancellation Support**: Graceful handling of Ctrl+C interruptions

## Installation

### Prerequisites
- .NET 9.0 SDK
- Azure OpenAI resource with a deployed model
- Azure account with appropriate permissions

### Setup

1. **Clone or download the project**
    ```bash
    git clone <your-repository-url>
    cd Chatbot-App
    ```
2. **Install dependencies**
   The project uses the following NuGet packages (automatically restored):
  
     ```xml
      <PackageReference Include="Microsoft.ML.Tokenizers" Version="2.0.0" />
      <PackageReference Include="Azure.AI.Projects" Version="1.1.0" />
      <PackageReference Include="Azure.Identity" Version="1.17.1" />
      <PackageReference Include="DotNetEnv" Version="3.1.1" />
      <PackageReference Include="Microsoft.ML.Tokenizers.Data.Cl100kBase" Version="2.0.0" />
      <PackageReference Include="Serilog" Version="4.1.0" />
      <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
      <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
      <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.0" />
      <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="9.0.0" />
      <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    ```

    
   Restore packages:
   ```bash
   dotnet restore
   ```

3. **Configure environment variables**
   ```bash
   cp .env.example .env
   ```
   
   Edit `.env` with your Azure OpenAI credentials:
   ```bash
   PROJECT_ENDPOINT=https://your-project.cognitiveservices.azure.com/
   MODEL_DEPLOYMENT_NAME=gpt-4
   API_KEY=your-api-key-here
   ```

4. **Build the application**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

## Configuration

### Set Up Azure AI Foundry

1. Navigate to the [Azure AI Foundry Portal](https://ai.azure.com)
2. Sign in with your Azure credentials
3. Create a new project or use an existing one
4. Deploy a model (e.g., GPT-4, GPT-4o, GPT-3.5-turbo)
5. Note the following from your project:
   - **Project Endpoint** (from Overview page → Endpoints and keys)
   - **Model Deployment Name** (from Models and endpoints page)
   - **API Key** (from your Azure OpenAI resource in Azure Portal)
## 
All configuration is managed through environment variables in the `.env` file:

### Required Variables
- `PROJECT_ENDPOINT` - Your Azure OpenAI project endpoint URL
- `MODEL_DEPLOYMENT_NAME` - Name of your deployed model
- `API_KEY` - Your Azure OpenAI API key

### Optional Variables (with defaults)
- `MAX_OUTPUT_TOKENS=4096` - Maximum tokens in responses
- `TEMPERATURE=1.0` - Response randomness (0-2)
- `TOP_P=1.0` - Nucleus sampling parameter (0-1)
- `MAX_HISTORY_MESSAGES=50` - Maximum conversation history
- `SYSTEM_PROMPT` - Custom system prompt for the assistant

## Architecture

### Project Structure
```
Chatbot-App/
├── Configuration/
│   ├── ChatbotConfiguration.cs    # Configuration model with validation
│   └── ConfigurationLoader.cs     # Environment variable loader
├── Interfaces/
│   ├── IChatService.cs            # ChatService interfaces
│   ├── ICommandHandler.cs         # CommandHandler interface
│   ├── IConsoleService.cs         # ConsoleService interface
│   ├── IConversationService.cs    # ConversationService interface
├── Services/
│   ├── ChatService.cs             # Azure OpenAI client wrapper
│   ├── CommandHandler.cs          # Command processing
│   ├── ConsoleService.cs          # Console I/O abstraction
│   └── ConversationService.cs     # Message history management
├── ChatbotClient.cs               # Main chat loop orchestrator
├── Program.cs                     # Application entry point with DI
└── .env                          # Configuration file
```

### Design Patterns
- **Dependency Injection**: All services registered and injected
- **Interface Segregation**: Clean abstractions for testability
- **Single Responsibility**: Each class has one clear purpose
- **Repository Pattern**: ConversationService handles data operations

## Error Handling

The application handles various error scenarios:

- **Authentication Errors** (401): Invalid API key
- **Authorization Errors** (403): Insufficient permissions
- **Not Found Errors** (404): Invalid model deployment
- **Rate Limiting** (429): Automatic retry with backoff
- **Server Errors** (500-504): Retry with clear messaging
- **Network Errors**: Automatic retry for transient failures
- **Configuration Errors**: Validation at startup with helpful messages

## Logging

Logs are written to:
- **Console**: INFO level and above
- **Files**: `logs/chatbot-YYYYMMDD.log` (7-day retention)

Log levels:
- **Information**: Normal operations
- **Warning**: Recoverable issues
- **Error**: Failed operations with stack traces
- **Fatal**: Unrecoverable errors causing shutdown


## Troubleshooting

### Common Issues

**"Configuration file not found"**
- Ensure `.env` file exists in the project root
- Check file permissions

**"Required environment variable not set"**
- Verify all required variables are in `.env`
- Check for typos in variable names

**"Authentication failed"**
- Verify API_KEY is correct
- Check Azure subscription status
- Ensure DefaultAzureCredential has access

**"Model deployment not found"**
- Verify MODEL_DEPLOYMENT_NAME matches Azure deployment
- Check model is deployed and available

## Performance Considerations

- **Token Limits**: Monitor estimated token counts with `stats` command
- **History Management**: Automatic trimming prevents context overflow
- **Streaming**: Reduces perceived latency for long responses

## Security

- **Never commit `.env`** to version control
- **Use Azure Key Vault** for production secrets
- **Rotate API keys** regularly
- **Use Managed Identity** when possible instead of API keys
- **Audit logs** for sensitive conversations

## License

This project is provided as-is for educational and development purposes.

## Support

For issues or questions:
1. Check the logs in `logs/` directory
2. Review error messages carefully
3. Verify configuration settings
4. Check Azure OpenAI service status
5. Review Azure subscription quotas

## Additional Resources

- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-studio/)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)
- [Azure.AI.Projects SDK](https://learn.microsoft.com/dotnet/api/azure.ai.projects)
- [OpenAI API Reference](https://platform.openai.com/docs/api-reference)

## Version History

- **v2.0.0** - Initial release
  - Streaming chat functionality
  - Command-line interface
  - Conversation history management
  - Color-coded console output

---
