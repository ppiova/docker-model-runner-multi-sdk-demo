"""
Example 02: Same local model via Anthropic SDK.

Docker Model Runner exposes an Anthropic-compatible endpoint at
/v1/messages. The Anthropic Python SDK sees it as a regular Anthropic
endpoint. Same model, same memory as in example 01, just a different
protocol on the same port.

Run:
    python examples/02_anthropic_local.py
"""

import os
from dotenv import load_dotenv
import anthropic

load_dotenv()

DMR_BASE_URL = os.getenv("DMR_BASE_URL", "http://localhost:12434")
MODEL = os.getenv("LOCAL_MODEL", "ai/qwen3-coder")


def main() -> None:
    # Note the path: the Anthropic mount point is the root, /v1/messages
    client = anthropic.Anthropic(
        base_url=DMR_BASE_URL,
        api_key="not-needed",  # DMR does not require auth
    )

    print(f"Calling {MODEL} via Anthropic SDK against DMR at {DMR_BASE_URL}\n")

    message = client.messages.create(
        model=MODEL,
        max_tokens=200,
        system="You are a concise technical writer. Reply in 2 sentences.",
        messages=[
            {
                "role": "user",
                "content": "Explain what Docker Model Runner is and why it matters.",
            },
        ],
    )

    print("Response:")
    # Anthropic returns content as a list of content blocks
    for block in message.content:
        if block.type == "text":
            print(block.text)

    print(
        f"\nTokens used: input={message.usage.input_tokens} "
        f"output={message.usage.output_tokens}"
    )


if __name__ == "__main__":
    main()
