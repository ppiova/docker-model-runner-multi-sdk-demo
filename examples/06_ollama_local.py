"""
Example 06: Same local model via Ollama SDK.

Docker Model Runner exposes an Ollama-compatible endpoint at /api/chat.
The Ollama Python SDK sees it as a regular Ollama server. Same model,
same memory as in examples 01 and 02, just a different protocol.

This completes the picture: one model in memory, three SDK protocols,
zero proxies.

Run:
    python examples/06_ollama_local.py
"""

import os
from dotenv import load_dotenv
import ollama

load_dotenv()

DMR_BASE_URL = os.getenv("DMR_BASE_URL", "http://localhost:12434")
MODEL = os.getenv("LOCAL_MODEL", "ai/qwen3-coder")


def main() -> None:
    client = ollama.Client(host=DMR_BASE_URL)

    print(f"Calling {MODEL} via Ollama SDK against DMR at {DMR_BASE_URL}\n")

    response = client.chat(
        model=MODEL,
        messages=[
            {
                "role": "user",
                "content": "Explain what Docker Model Runner is and why it matters.",
            },
        ],
        options={"num_predict": 200},
    )

    print("Response:")
    print(response.message.content)
    print(
        f"\nTokens used: input={response.prompt_eval_count} "
        f"output={response.eval_count}"
    )


if __name__ == "__main__":
    main()
