#!/usr/bin/env bash
#
# Example 05: Raw API calls to the three protocols DMR speaks.
#
# Same model, same port, three different request/response formats.
# Useful for debugging, for documentation, and for showing what each
# SDK sends under the hood.
#
# Run:
#     bash examples/05_curl_examples.sh

set -euo pipefail

DMR_BASE_URL="${DMR_BASE_URL:-http://localhost:12434}"
MODEL="${LOCAL_MODEL:-ai/qwen3-coder}"

echo "==============================================================="
echo "DMR endpoint: $DMR_BASE_URL"
echo "Model:        $MODEL"
echo "==============================================================="

echo
echo "--- 1) OpenAI Chat Completions (POST /engines/v1/chat/completions) ---"
curl -s "$DMR_BASE_URL/engines/v1/chat/completions" \
  -H "Content-Type: application/json" \
  -d "{
    \"model\": \"$MODEL\",
    \"messages\": [
      {\"role\": \"user\", \"content\": \"Say hello in exactly 5 words.\"}
    ],
    \"max_tokens\": 50
  }" | python -m json.tool

echo
echo "--- 2) Anthropic Messages (POST /v1/messages) ---"
curl -s "$DMR_BASE_URL/v1/messages" \
  -H "Content-Type: application/json" \
  -d "{
    \"model\": \"$MODEL\",
    \"max_tokens\": 50,
    \"messages\": [
      {\"role\": \"user\", \"content\": \"Say hello in exactly 5 words.\"}
    ]
  }" | python -m json.tool

echo
echo "--- 3) Ollama Chat (POST /api/chat) ---"
curl -s "$DMR_BASE_URL/api/chat" \
  -H "Content-Type: application/json" \
  -d "{
    \"model\": \"$MODEL\",
    \"messages\": [
      {\"role\": \"user\", \"content\": \"Say hello in exactly 5 words.\"}
    ],
    \"stream\": false
  }" | python -m json.tool

echo
echo "--- 4) List models (GET /engines/v1/models) ---"
curl -s "$DMR_BASE_URL/engines/v1/models" | python -m json.tool

echo
echo "Three protocols, one model, one runtime."
