/*
 * Example 04: Dev/prod parity with Microsoft Foundry.
 *
 * The killer pattern. Same client code, same SDK calls, same prompt.
 * The only thing that changes between development and production is
 * the endpoint and credentials.
 *
 * Set ENVIRONMENT=development to hit local DMR.
 * Set ENVIRONMENT=production to hit Microsoft Foundry (Azure OpenAI)
 * and Anthropic API.
 *
 * Run:
 *     dotnet run --project examples/dotnet -- 04
 *     ENVIRONMENT=production dotnet run --project examples/dotnet -- 04
 */

internal static class Example04_DevProdParity
{
    const string Prompt = "Summarize the benefits of running models locally during development in 3 bullets.";

    public static async Task RunAsync()
    {
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "development";
        var dmrBaseUrl  = Environment.GetEnvironmentVariable("DMR_BASE_URL") ?? "http://localhost:12434";
        var localModel  = Environment.GetEnvironmentVariable("LOCAL_MODEL")   ?? "ai/qwen3-coder";

        Console.WriteLine($"Running in {environment.ToUpper()} mode\n");
        Console.WriteLine("Same SDK calls in both modes. Only base_url and credentials change.");

        await CallViaOpenAISdk(environment, dmrBaseUrl, localModel);
        await CallViaAnthropicSdk(environment, dmrBaseUrl, localModel);

        Console.WriteLine(
            "\nThe code path is identical regardless of environment. " +
            "Local dev costs nothing. Production hits Foundry.");
    }

    static async Task CallViaOpenAISdk(string environment, string dmrBaseUrl, string localModel)
    {
        var (chatClient, model) = GetOpenAIChatClient(environment, dmrBaseUrl, localModel);
        Console.WriteLine($"\n[OpenAI SDK] env={environment} model={model}");

        var response = await chatClient.CompleteChatAsync([new UserChatMessage(Prompt)]);
        Console.WriteLine(response.Value.Content[0].Text);
    }

    static async Task CallViaAnthropicSdk(string environment, string dmrBaseUrl, string localModel)
    {
        var (client, model) = GetAnthropicClient(environment, dmrBaseUrl, localModel);
        Console.WriteLine($"\n[Anthropic SDK] env={environment} model={model}");

        var parameters = new MessageCreateParams
        {
            Model     = model,
            MaxTokens = 300,
            Messages  = [new() { Role = Role.User, Content = Prompt }],
        };

        var message = await client.Messages.Create(parameters);
        var text = string.Concat(
            message.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
        Console.WriteLine(text);
    }

    // Returns a ChatClient that already has the correct endpoint baked in.
    // In .NET, both OpenAIClient and AzureOpenAIClient produce the same ChatClient type,
    // so there is no union type needed — the caller just uses ChatClient directly.
    static (ChatClient client, string model) GetOpenAIChatClient(
        string environment, string dmrBaseUrl, string localModel)
    {
        if (environment == "production")
        {
            var endpoint   = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
            var apiKey     = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
            var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "your-deployment-name";

            var azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
            return (azureClient.GetChatClient(deployment), deployment);
        }
        else
        {
            var client = new OpenAIClient(
                new ApiKeyCredential("not-needed"),
                new OpenAIClientOptions { Endpoint = new Uri($"{dmrBaseUrl}/engines/v1") }
            );
            return (client.GetChatClient(localModel), localModel);
        }
    }

    static (AnthropicClient client, string model) GetAnthropicClient(
        string environment, string dmrBaseUrl, string localModel)
    {
        if (environment == "production")
        {
            // Clear the dev-mode base URL override; use the real API key from .env.
            Environment.SetEnvironmentVariable("ANTHROPIC_BASE_URL", null);
            var model = Environment.GetEnvironmentVariable("ANTHROPIC_MODEL") ?? "claude-opus-4-7";
            return (new AnthropicClient(), model);
        }
        else
        {
            Environment.SetEnvironmentVariable("ANTHROPIC_BASE_URL", dmrBaseUrl);
            Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", "not-needed");
            return (new AnthropicClient(), localModel);
        }
    }
}
