# One Endpoint, Three SDKs

A minimal demo showing how **Docker Model Runner (DMR)** exposes the same local model through three native API formats (OpenAI, Anthropic, Ollama) on a single endpoint, and how that unlocks dev/prod parity with **Microsoft Foundry**.

> 🇪🇸 [Versión en español](./README.es.md)

## Why this matters

Most local LLM runtimes speak one protocol. DMR speaks three, natively, on the same port. That means:

- One model in memory, consumed by the OpenAI SDK, the Anthropic SDK, or any Ollama-compatible client.
- No translation proxy, no LiteLLM in the middle, zero extra hops.
- Same SDK code in development (against local DMR) and production (against Microsoft Foundry or Anthropic API). One env var changes.

This repo proves it with six short, runnable examples.

## Prerequisites

- **Docker Desktop 4.58.0+** (Anthropic API support requires this version)
- **Python 3.10+**
- **GPU optional**, llama.cpp runs on CPU and Apple Silicon

## Setup

### 1. Enable Docker Model Runner

```bash
# Enable DMR with TCP access on port 12434
docker desktop enable model-runner --tcp 12434

# Verify it is running
curl http://localhost:12434/engines/v1/models
```

### 2. Pull a model

Pick one. The examples default to `ai/qwen3-coder` (good for coding, 128K context).

```bash
docker model pull ai/qwen3-coder
# Alternatives:
# docker model pull ai/llama3.2          # smaller, faster
# docker model pull ai/devstral-small-2  # coding-focused
# docker model pull ai/mistral           # general purpose
```

### 3. Install Python dependencies

```bash
python -m venv .venv
source .venv/bin/activate          # Windows: .venv\Scripts\activate
pip install -r requirements.txt
```

### 4. Configure environment

```bash
cp .env.example .env
# Edit .env if you want to test the production-parity example
```

## Run the examples

### `01_openai_local.py` — Local model via OpenAI SDK

```bash
python examples/01_openai_local.py
```

DMR exposes `/engines/v1/chat/completions`. The OpenAI Python SDK sees it as a regular OpenAI endpoint. No translation.

### `02_anthropic_local.py` — Same model via Anthropic SDK

```bash
python examples/02_anthropic_local.py
```

DMR exposes `/v1/messages`. The Anthropic Python SDK sees it as a regular Anthropic endpoint. **Same model, same memory, just a different protocol.**

### `03_streaming.py` — Streaming with both SDKs

```bash
python examples/03_streaming.py
```

Token-by-token streaming works identically on both protocols.

### `04_dev_prod_parity.py` — The killer pattern

```bash
# Development: hit local DMR
ENVIRONMENT=development python examples/04_dev_prod_parity.py

# Production: hit Microsoft Foundry (requires .env values)
ENVIRONMENT=production python examples/04_dev_prod_parity.py
```

Same client code, same SDK calls. Only the `base_url` and credentials change.

### `05_curl_examples.sh` — Raw API calls

```bash
bash examples/05_curl_examples.sh
```

Hit the three protocols directly with curl. Useful for debugging and for understanding what each SDK sends under the hood.

### `06_ollama_local.py` — Same model via Ollama SDK

```bash
python examples/06_ollama_local.py
```

DMR exposes `/api/chat`. The Ollama Python SDK sees it as a regular Ollama server. **Same model, same memory, third protocol.** This completes the picture: one model in memory, three Python SDKs, zero proxies.

## What the demo proves

| Pattern | Demonstrated by |
|---|---|
| One model, three SDKs, no proxy | `01`, `02`, `06` |
| Streaming parity across protocols | `03` |
| Dev/prod parity with Foundry | `04` |
| Raw protocol inspection | `05` |

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Your laptop                          │
│                                                         │
│  ┌──────────────┐      ┌────────────────────────────┐   │
│  │  OpenAI SDK  │─────▶│ /engines/v1/chat/completions│   │
│  └──────────────┘      │                            │   │
│                        │   Docker Model Runner      │   │
│  ┌──────────────┐      │                            │   │
│  │ Anthropic SDK│─────▶│       /v1/messages         │   │
│  └──────────────┘      │                            │   │
│                        │   ┌──────────────────────┐ │   │
│  ┌──────────────┐      │   │ ai/qwen3-coder       │ │   │
│  │ Claude Code  │─────▶│   │ (one model, in memory)│ │   │
│  └──────────────┘      │   └──────────────────────┘ │   │
│                        └────────────────────────────┘   │
│                              localhost:12434            │
└─────────────────────────────────────────────────────────┘
                              │
                              │ same SDK calls, swap base_url
                              ▼
                   ┌──────────────────────┐
                   │   Microsoft Foundry  │
                   │   (production)       │
                   └──────────────────────┘
```

## Endpoint reference

| Protocol | Path | Used by |
|---|---|---|
| OpenAI Chat Completions | `/engines/v1/chat/completions` | `openai`, `langchain-openai`, AI SDK |
| OpenAI Responses | `/engines/v1/responses` | `openai` (reasoning models) |
| Anthropic Messages | `/v1/messages` | `anthropic`, Claude Code |
| Anthropic Token Count | `/v1/messages/count_tokens` | `anthropic` |
| Ollama Chat | `/api/chat` | Ollama clients |

## Resources

- [Docker Model Runner docs](https://docs.docker.com/ai/model-runner/)
- [DMR REST API reference](https://docs.docker.com/ai/model-runner/api-reference/)
- [Microsoft Foundry](https://ai.azure.com/)
- [Anthropic Messages API](https://docs.claude.com/en/api/messages)

## License

MIT

---

Built as a companion to a LinkedIn article on `#MicrosoftFoundry` and local AI development.
