// Load .env from the repo root, traversing up from the current directory.
DotNetEnv.Env.TraversePath().Load();

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run --project examples/dotnet -- <example>");
    Console.WriteLine();
    Console.WriteLine("  01  Local model via OpenAI SDK");
    Console.WriteLine("  02  Same model via Anthropic SDK (official)");
    Console.WriteLine("  03  Streaming with both SDKs");
    Console.WriteLine("  04  Dev/prod parity with Microsoft Foundry");
    Console.WriteLine("  06  Same model via Ollama SDK");
    return;
}

await (args[0] switch
{
    "01" => Example01_OpenAI.RunAsync(),
    "02" => Example02_Anthropic.RunAsync(),
    "03" => Example03_Streaming.RunAsync(),
    "04" => Example04_DevProdParity.RunAsync(),
    "06" => Example06_Ollama.RunAsync(),
    _    => throw new ArgumentException($"Unknown example '{args[0]}'. Valid: 01 02 03 04 06"),
});
