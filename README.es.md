# Un endpoint, tres SDKs

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Python](https://img.shields.io/badge/Python-3.10%2B-blue?logo=python&logoColor=white)](https://www.python.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker_Model_Runner-4.58%2B-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/ai/model-runner/)

Demo mínima que muestra cómo **Docker Model Runner (DMR)** expone el mismo modelo local a través de tres formatos de API nativos (OpenAI, Anthropic, Ollama) en un único endpoint, y cómo eso habilita paridad dev/prod con **Microsoft Foundry**.

> 🇬🇧 [English version](./README.md)

## Por qué importa

La mayoría de los runtimes locales de LLM hablan un solo protocolo. DMR habla tres, nativamente, en el mismo puerto. Esto significa:

- Un modelo en memoria, consumido por el SDK de OpenAI, el SDK de Anthropic, o cualquier cliente compatible con Ollama.
- Sin proxy de traducción, sin LiteLLM en el medio, cero hops adicionales.
- Mismo código de SDK en desarrollo (contra DMR local) y en producción (contra Microsoft Foundry o Anthropic API). Cambia una sola variable de entorno.

Este repo lo demuestra con seis ejemplos cortos y ejecutables.

## Requisitos

- **Docker Desktop 4.58.0+** (el soporte de API Anthropic requiere esta versión)
- **Python 3.10+** (para los ejemplos en Python)
- **.NET 10 SDK** (para los ejemplos en C# — proyecto en `examples/dotnet`)
- **GPU opcional**, llama.cpp corre en CPU y Apple Silicon

## Setup

### 1. Habilitar Docker Model Runner

```bash
# Habilitar DMR con acceso TCP en el puerto 12434
docker desktop enable model-runner --tcp 12434

# Verificar que está corriendo
curl http://localhost:12434/engines/v1/models
```

### 2. Bajar un modelo

Elegí uno. Los ejemplos usan por defecto `ai/qwen3-coder` (bueno para código, contexto 128K).

```bash
docker model pull ai/qwen3-coder
# Alternativas:
# docker model pull ai/llama3.2          # más chico, más rápido
# docker model pull ai/devstral-small-2  # enfocado en código
# docker model pull ai/mistral           # propósito general
```

### 3. Instalar dependencias Python

```bash
python -m venv .venv
source .venv/bin/activate          # Windows: .venv\Scripts\activate
pip install -r requirements.txt
```

### 4. Configurar entorno

```bash
cp .env.example .env
# Editá .env si querés probar el ejemplo de paridad con producción
```

## Ejecutar los ejemplos

### `01_openai_local.py` — Modelo local vía SDK OpenAI

```bash
python examples/01_openai_local.py
```

DMR expone `/engines/v1/chat/completions`. El SDK Python de OpenAI lo ve como un endpoint OpenAI normal. Sin traducción.

### `02_anthropic_local.py` — Mismo modelo vía SDK Anthropic

```bash
python examples/02_anthropic_local.py
```

DMR expone `/v1/messages`. El SDK Python de Anthropic lo ve como un endpoint Anthropic normal. **Mismo modelo, misma memoria, solo cambia el protocolo.**

### `03_streaming.py` — Streaming con ambos SDKs

```bash
python examples/03_streaming.py
```

Streaming token a token funciona idéntico en ambos protocolos.

### `04_dev_prod_parity.py` — El patrón killer

```bash
# Desarrollo: pega contra DMR local
ENVIRONMENT=development python examples/04_dev_prod_parity.py

# Producción: pega contra Microsoft Foundry (requiere valores en .env)
ENVIRONMENT=production python examples/04_dev_prod_parity.py
```

Mismo código de cliente, mismas llamadas al SDK. Solo cambian `base_url` y credenciales.

### `05_curl_examples.sh` — Llamadas raw a la API

```bash
bash examples/05_curl_examples.sh
```

Pega los tres protocolos directamente con curl. Útil para debug y para entender qué manda cada SDK por debajo.

### `06_ollama_local.py` — Mismo modelo vía SDK Ollama

```bash
python examples/06_ollama_local.py
```

DMR expone `/api/chat`. El SDK Python de Ollama lo ve como un servidor Ollama normal. **Mismo modelo, misma memoria, tercer protocolo.** Esto cierra el círculo: un modelo en memoria, tres SDKs de Python, cero proxies.

## Ejemplos en .NET 10 / C#

Los mismos cinco ejemplos están disponibles como proyecto de consola .NET 10 en `examples/dotnet/`.

**Paquetes usados:**
| SDK | Paquete NuGet | Versión |
|---|---|---|
| OpenAI | `OpenAI` | 2.10.0 |
| Azure OpenAI (Foundry) | `Azure.AI.OpenAI` | 2.1.0 |
| Anthropic (oficial) | `Anthropic` | 12.17.0 |
| Ollama | `OllamaSharp` | 5.4.25 |

**Ejecutar:**

```bash
# Restaurar dependencias
dotnet restore examples/dotnet

# Elegir un ejemplo
dotnet run --project examples/dotnet -- 01   # SDK OpenAI
dotnet run --project examples/dotnet -- 02   # SDK Anthropic (oficial)
dotnet run --project examples/dotnet -- 03   # Streaming (ambos SDKs)
dotnet run --project examples/dotnet -- 04   # Paridad dev/prod
ENVIRONMENT=production dotnet run --project examples/dotnet -- 04
dotnet run --project examples/dotnet -- 06   # SDK Ollama
```

El proyecto lee el mismo archivo `.env` de la raíz del repo via `DotNetEnv.Env.TraversePath().Load()`, sin configuración adicional.

## Qué demuestra la demo

| Patrón | Demostrado por |
|---|---|
| Un modelo, tres SDKs, sin proxy | `01`, `02`, `06` |
| Paridad de streaming entre protocolos | `03` |
| Paridad dev/prod con Foundry | `04` |
| Inspección raw de protocolos | `05` |

## Arquitectura

```
┌─────────────────────────────────────────────────────────┐
│                     Tu laptop                           │
│                                                         │
│  ┌──────────────┐      ┌────────────────────────────┐   │
│  │  SDK OpenAI  │─────▶│ /engines/v1/chat/completions│   │
│  └──────────────┘      │                            │   │
│                        │   Docker Model Runner      │   │
│  ┌──────────────┐      │                            │   │
│  │ SDK Anthropic│─────▶│       /v1/messages         │   │
│  └──────────────┘      │                            │   │
│                        │   ┌──────────────────────┐ │   │
│  ┌──────────────┐      │   │ ai/qwen3-coder       │ │   │
│  │ Claude Code  │─────▶│   │ (un modelo, en RAM)  │ │   │
│  └──────────────┘      │   └──────────────────────┘ │   │
│                        └────────────────────────────┘   │
│                              localhost:12434            │
└─────────────────────────────────────────────────────────┘
                              │
                              │ mismas llamadas SDK, cambia base_url
                              ▼
                   ┌──────────────────────┐
                   │   Microsoft Foundry  │
                   │   (producción)       │
                   └──────────────────────┘
```

## Referencia de endpoints

| Protocolo | Path | Usado por |
|---|---|---|
| OpenAI Chat Completions | `/engines/v1/chat/completions` | `openai`, `langchain-openai`, AI SDK |
| OpenAI Responses | `/engines/v1/responses` | `openai` (modelos reasoning) |
| Anthropic Messages | `/v1/messages` | `anthropic`, Claude Code |
| Anthropic Token Count | `/v1/messages/count_tokens` | `anthropic` |
| Ollama Chat | `/api/chat` | clientes Ollama |

## Recursos

- [Documentación de Docker Model Runner](https://docs.docker.com/ai/model-runner/)
- [Referencia REST API de DMR](https://docs.docker.com/ai/model-runner/api-reference/)
- [Microsoft Foundry](https://ai.azure.com/)
- [Anthropic Messages API](https://docs.claude.com/en/api/messages)

## Licencia

MIT

## Sobre el autor

**Pablo Piovano** — Director of AI at [OZ Digital](https://ozdigital.ai) · Microsoft MVP en IA

[![LinkedIn](https://img.shields.io/badge/LinkedIn-ppiova-0A66C2?logo=linkedin&logoColor=white)](https://www.linkedin.com/in/ppiova/)

---

Construido como complemento de un artículo de LinkedIn sobre `#MicrosoftFoundry` y desarrollo local de IA.
