/*
 * Example 02: Same local model via Anthropic SDK (official).
 *
 * Docker Model Runner exposes an Anthropic-compatible endpoint at /v1/messages.
 * The official Anthropic .NET SDK sees it as a regular Anthropic endpoint.
 * Same model, same memory as in example 01, just a different protocol on the
 * same port.
 *
 * The SDK resolves ANTHROPIC_BASE_URL and ANTHROPIC_API_KEY from the environment.
 *
 * Run:
 *     dotnet run --project examples/dotnet -- 02
 */

internal static class Example02_Anthropic
{
    public static async Task RunAsync()
    {
        var dmrBaseUrl = Environment.GetEnvironmentVariable("DMR_BASE_URL") ?? "http://localhost:12434";
        var model      = Environment.GetEnvironmentVariable("LOCAL_MODEL")   ?? "ai/qwen3-coder";

        // Point the official SDK at DMR. The SDK reads these env vars automatically.
        Environment.SetEnvironmentVariable("ANTHROPIC_BASE_URL", dmrBaseUrl);
        Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", "not-needed");

        using var client = new AnthropicClient();

        Console.WriteLine($"Calling {model} via Anthropic SDK against DMR at {dmrBaseUrl}\n");

        var parameters = new MessageCreateParams
        {
            Model     = model,
            MaxTokens = 200,
            System    = "You are a concise technical writer. Reply in 2 sentences.",
            Messages  =
            [
                new() { Role = Role.User, Content = "Explain what Docker Model Runner is and why it matters." },
            ],
        };

        var message = await client.Messages.Create(parameters);

        Console.WriteLine("Response:");
        var text = string.Concat(
            message.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
        Console.WriteLine(text);
        Console.WriteLine($"\nTokens used: input={message.Usage.InputTokens} output={message.Usage.OutputTokens}");
    }
}
