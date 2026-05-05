/*
 * Example 01: Local model via OpenAI SDK.
 *
 * Docker Model Runner exposes an OpenAI-compatible endpoint at
 * /engines/v1/chat/completions. The OpenAI .NET SDK sees it as a
 * regular OpenAI endpoint. No proxy, no translation.
 *
 * Run:
 *     dotnet run --project examples/dotnet -- 01
 */

internal static class Example01_OpenAI
{
    public static async Task RunAsync()
    {
        var dmrBaseUrl = Environment.GetEnvironmentVariable("DMR_BASE_URL") ?? "http://localhost:12434";
        var model      = Environment.GetEnvironmentVariable("LOCAL_MODEL")   ?? "ai/qwen3-coder";

        // /engines/v1 is the OpenAI-compatible mount point on DMR.
        var client = new OpenAIClient(
            new ApiKeyCredential("not-needed"),
            new OpenAIClientOptions { Endpoint = new Uri($"{dmrBaseUrl}/engines/v1") }
        );
        var chatClient = client.GetChatClient(model);

        Console.WriteLine($"Calling {model} via OpenAI SDK against DMR at {dmrBaseUrl}\n");

        var response = await chatClient.CompleteChatAsync(
        [
            new SystemChatMessage("You are a concise technical writer. Reply in 2 sentences."),
            new UserChatMessage("Explain what Docker Model Runner is and why it matters."),
        ]);

        Console.WriteLine("Response:");
        Console.WriteLine(response.Value.Content[0].Text);
        Console.WriteLine($"\nTokens used: {response.Value.Usage.TotalTokenCount}");
    }
}
