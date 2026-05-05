/*
 * Example 03: Streaming with both SDKs against the same local model.
 *
 * Both protocols support token-by-token streaming. This example runs the
 * same prompt twice — once via OpenAI SDK and once via Anthropic SDK —
 * streaming the output to stdout. The model is loaded once and reused.
 *
 * Run:
 *     dotnet run --project examples/dotnet -- 03
 */

internal static class Example03_Streaming
{
    const string Prompt = "Write a haiku about Docker containers.";

    public static async Task RunAsync()
    {
        var dmrBaseUrl = Environment.GetEnvironmentVariable("DMR_BASE_URL") ?? "http://localhost:12434";
        var model      = Environment.GetEnvironmentVariable("LOCAL_MODEL")   ?? "ai/qwen3-coder";

        Console.WriteLine($"Model: {model}");
        Console.WriteLine($"Endpoint: {dmrBaseUrl}\n");

        await StreamViaOpenAISdk(dmrBaseUrl, model);
        await StreamViaAnthropicSdk(dmrBaseUrl, model);

        Console.WriteLine("Same model, same memory, two protocols. No proxy in the middle.");
    }

    static async Task StreamViaOpenAISdk(string dmrBaseUrl, string model)
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("Streaming via OpenAI SDK");
        Console.WriteLine(new string('=', 60));

        var client = new OpenAIClient(
            new ApiKeyCredential("not-needed"),
            new OpenAIClientOptions { Endpoint = new Uri($"{dmrBaseUrl}/engines/v1") }
        );
        var chatClient = client.GetChatClient(model);

        await foreach (var update in chatClient.CompleteChatStreamingAsync(
            [new UserChatMessage(Prompt)]))
        {
            foreach (var part in update.ContentUpdate)
                Console.Write(part.Text);
        }

        Console.WriteLine("\n");
    }

    static async Task StreamViaAnthropicSdk(string dmrBaseUrl, string model)
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("Streaming via Anthropic SDK");
        Console.WriteLine(new string('=', 60));

        Environment.SetEnvironmentVariable("ANTHROPIC_BASE_URL", dmrBaseUrl);
        Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", "not-needed");

        using var client = new AnthropicClient();

        var parameters = new MessageCreateParams
        {
            Model     = model,
            MaxTokens = 200,
            Messages  = [new() { Role = Role.User, Content = Prompt }],
        };

        // CreateStreaming returns IAsyncEnumerable<RawMessageStreamEvent>.
        // TryPickContentBlockDelta + TryPickText extracts the streaming text tokens.
        await foreach (var rawEvent in client.Messages.CreateStreaming(parameters))
        {
            if (rawEvent.TryPickContentBlockDelta(out var delta) &&
                delta.Delta.TryPickText(out var text))
            {
                Console.Write(text.Text);
            }
        }

        Console.WriteLine("\n");
    }
}
