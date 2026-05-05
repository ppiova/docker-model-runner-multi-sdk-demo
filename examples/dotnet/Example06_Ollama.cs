/*
 * Example 06: Same local model via Ollama SDK (OllamaSharp).
 *
 * Docker Model Runner exposes an Ollama-compatible endpoint at /api/chat.
 * OllamaSharp sees it as a regular Ollama server. Same model, same memory
 * as in examples 01 and 02, just a different protocol.
 *
 * This completes the picture: one model in memory, three SDK protocols,
 * zero proxies.
 *
 * Run:
 *     dotnet run --project examples/dotnet -- 06
 */

internal static class Example06_Ollama
{
    public static async Task RunAsync()
    {
        var dmrBaseUrl = Environment.GetEnvironmentVariable("DMR_BASE_URL") ?? "http://localhost:12434";
        var model      = Environment.GetEnvironmentVariable("LOCAL_MODEL")   ?? "ai/qwen3-coder";

        var ollama = new OllamaApiClient(new Uri(dmrBaseUrl)) { SelectedModel = model };

        Console.WriteLine($"Calling {model} via Ollama SDK against DMR at {dmrBaseUrl}\n");

        var chat = new Chat(ollama);
        var sb   = new StringBuilder();

        await foreach (var token in chat.SendAsync(
            "Explain what Docker Model Runner is and why it matters."))
        {
            sb.Append(token);
        }

        Console.WriteLine("Response:");
        Console.WriteLine(sb.ToString());
    }
}
