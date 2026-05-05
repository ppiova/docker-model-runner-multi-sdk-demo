"""
Example 04: Dev/prod parity with Microsoft Foundry.

The killer pattern. Same client code, same SDK calls, same prompt.
The only thing that changes between development and production is
the base_url and credentials.

Set ENVIRONMENT=development to hit local DMR.
Set ENVIRONMENT=production to hit Microsoft Foundry (Azure OpenAI)
and Anthropic API.

Run:
    ENVIRONMENT=development python examples/04_dev_prod_parity.py
    ENVIRONMENT=production  python examples/04_dev_prod_parity.py
"""

import os
from dotenv import load_dotenv
from openai import OpenAI, AzureOpenAI
import anthropic

load_dotenv()

ENVIRONMENT = os.getenv("ENVIRONMENT", "development")
DMR_BASE_URL = os.getenv("DMR_BASE_URL", "http://localhost:12434")
LOCAL_MODEL = os.getenv("LOCAL_MODEL", "ai/qwen3-coder")

PROMPT = "Summarize the benefits of running models locally during development in 3 bullets."


def get_openai_client_and_model() -> tuple[OpenAI | AzureOpenAI, str]:
    """Return an OpenAI-compatible client and model name for the current environment."""
    if ENVIRONMENT == "production":
        # Production: Microsoft Foundry / Azure OpenAI
        client: OpenAI | AzureOpenAI = AzureOpenAI(
            azure_endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
            api_key=os.getenv("AZURE_OPENAI_API_KEY"),
            api_version=os.getenv("AZURE_OPENAI_API_VERSION", "2024-10-21"),
        )
        model = os.getenv("AZURE_OPENAI_DEPLOYMENT", "your-deployment-name")
    else:
        # Development: local DMR
        client = OpenAI(
            base_url=f"{DMR_BASE_URL}/engines/v1",
            api_key="not-needed",
        )
        model = LOCAL_MODEL

    return client, model


def get_anthropic_client_and_model() -> tuple[anthropic.Anthropic, str]:
    """Return an Anthropic client and model name for the current environment."""
    if ENVIRONMENT == "production":
        # Production: Anthropic API (or Foundry deployment hosting Claude)
        client = anthropic.Anthropic(api_key=os.getenv("ANTHROPIC_API_KEY"))
        model = os.getenv("ANTHROPIC_MODEL", "claude-opus-4-7")
    else:
        # Development: local DMR
        client = anthropic.Anthropic(
            base_url=DMR_BASE_URL,
            api_key="not-needed",
        )
        model = LOCAL_MODEL

    return client, model


def call_via_openai_sdk() -> None:
    client, model = get_openai_client_and_model()
    print(f"\n[OpenAI SDK] env={ENVIRONMENT} model={model}")

    response = client.chat.completions.create(
        model=model,
        messages=[{"role": "user", "content": PROMPT}],
        max_tokens=300,
    )
    print(response.choices[0].message.content)


def call_via_anthropic_sdk() -> None:
    client, model = get_anthropic_client_and_model()
    print(f"\n[Anthropic SDK] env={ENVIRONMENT} model={model}")

    message = client.messages.create(
        model=model,
        max_tokens=300,
        messages=[{"role": "user", "content": PROMPT}],
    )
    for block in message.content:
        if block.type == "text":
            print(block.text)


def main() -> None:
    print(f"Running in {ENVIRONMENT.upper()} mode\n")
    print("Same SDK calls in both modes. Only base_url and credentials change.")

    call_via_openai_sdk()
    call_via_anthropic_sdk()

    print(
        "\nThe code path is identical regardless of environment. "
        "Local dev costs nothing. Production hits Foundry."
    )


if __name__ == "__main__":
    main()
