"""
Example 01: Local model via OpenAI SDK.

Docker Model Runner exposes an OpenAI-compatible endpoint at
/engines/v1/chat/completions. The OpenAI Python SDK sees it as a
regular OpenAI endpoint. No proxy, no translation.

Run:
    python examples/01_openai_local.py
"""

import os
from dotenv import load_dotenv
from openai import OpenAI

load_dotenv()

DMR_BASE_URL = os.getenv("DMR_BASE_URL", "http://localhost:12434")
MODEL = os.getenv("LOCAL_MODEL", "ai/qwen3-coder")


def main() -> None:
    # Note the path: /engines/v1 is the OpenAI-compatible mount point
    client = OpenAI(
        base_url=f"{DMR_BASE_URL}/engines/v1",
        api_key="not-needed",  # DMR does not require auth
    )

    print(f"Calling {MODEL} via OpenAI SDK against DMR at {DMR_BASE_URL}\n")

    response = client.chat.completions.create(
        model=MODEL,
        messages=[
            {
                "role": "system",
                "content": "You are a concise technical writer. Reply in 2 sentences.",
            },
            {
                "role": "user",
                "content": "Explain what Docker Model Runner is and why it matters.",
            },
        ],
        max_tokens=200,
    )

    print("Response:")
    print(response.choices[0].message.content)
    print(f"\nTokens used: {response.usage.total_tokens}")


if __name__ == "__main__":
    main()
