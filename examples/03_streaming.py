"""
Example 03: Streaming with both SDKs against the same local model.

Both protocols support token-by-token streaming. This example runs the
same prompt twice, once via OpenAI SDK and once via Anthropic SDK,
streaming the output to stdout. The model is loaded once and reused.

Run:
    python examples/03_streaming.py
"""

import os
import sys
from dotenv import load_dotenv
from openai import OpenAI
import anthropic

load_dotenv()

DMR_BASE_URL = os.getenv("DMR_BASE_URL", "http://localhost:12434")
MODEL = os.getenv("LOCAL_MODEL", "ai/qwen3-coder")

PROMPT = "Write a haiku about Docker containers."


def stream_via_openai_sdk() -> None:
    print("=" * 60)
    print("Streaming via OpenAI SDK")
    print("=" * 60)

    client = OpenAI(
        base_url=f"{DMR_BASE_URL}/engines/v1",
        api_key="not-needed",
    )

    stream = client.chat.completions.create(
        model=MODEL,
        messages=[{"role": "user", "content": PROMPT}],
        max_tokens=200,
        stream=True,
    )

    for chunk in stream:
        if chunk.choices and chunk.choices[0].delta.content:
            sys.stdout.write(chunk.choices[0].delta.content)
            sys.stdout.flush()
    print("\n")


def stream_via_anthropic_sdk() -> None:
    print("=" * 60)
    print("Streaming via Anthropic SDK")
    print("=" * 60)

    client = anthropic.Anthropic(
        base_url=DMR_BASE_URL,
        api_key="not-needed",
    )

    with client.messages.stream(
        model=MODEL,
        max_tokens=200,
        messages=[{"role": "user", "content": PROMPT}],
    ) as stream:
        for text in stream.text_stream:
            sys.stdout.write(text)
            sys.stdout.flush()
    print("\n")


def main() -> None:
    print(f"\nModel: {MODEL}")
    print(f"Endpoint: {DMR_BASE_URL}\n")

    stream_via_openai_sdk()
    stream_via_anthropic_sdk()

    print("Same model, same memory, two protocols. No proxy in the middle.")


if __name__ == "__main__":
    main()
