# RVR AI RAG App

An AI-native application with RAG (Retrieval-Augmented Generation) pipeline, multi-AI provider support, and token cost tracking.

## Features

- **RAG Pipeline**: PDF document processing with vector embeddings
- **Multi-AI Providers**: OpenAI, Anthropic (Claude), Ollama (local)
- **Vector Store**: Qdrant for semantic search
- **Token Tracking**: Cost allocation per user, model, and provider
- **Blazor Chat UI**: Modern chat interface with conversation history
- **Document Upload**: PDF and text file processing

## Architecture

```
┌─────────────────┐     ┌─────────────────┐
│   Blazor WASM   │────▶│   API (REST)    │
│   (Port 5001)   │     │   (Port 5000)   │
└─────────────────┘     └────────┬────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   PostgreSQL    │     │    Qdrant       │     │    Ollama       │
│   (Metadata)    │     │  (Vector Store) │     │  (Local LLMs)   │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                                 │
                                 ▼
                        ┌─────────────────┐
                        │  OpenAI/Claude  │
                        │   (Cloud API)   │
                        └─────────────────┘
```

## Quick Start

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose
- API keys for OpenAI and/or Anthropic (optional)

### Using Docker Compose

```bash
cd samples/ai-rag-app

# Basic setup (uses cloud APIs)
docker-compose up -d

# With local Ollama models
docker-compose --profile ollama up -d
```

Access points:
- **Blazor UI**: http://localhost:5001
- **API Swagger**: http://localhost:5000/swagger
- **Qdrant Dashboard**: http://localhost:6333/dashboard

### Environment Variables

```bash
export OPENAI_API_KEY="sk-..."
export ANTHROPIC_API_KEY="sk-ant-..."
docker-compose up -d
```

## Project Structure

```
ai-rag-app/
├── src/
│   ├── RVR.AI.RAG.App.Api/          # REST API
│   ├── RVR.AI.RAG.App.Blazor/       # Blazor Chat UI
│   ├── RVR.AI.RAG.App.Application/  # DTOs, Commands
│   ├── RVR.AI.RAG.App.Domain/       # Entities
│   ├── RVR.AI.RAG.App.Infrastructure/ # EF Core, DI
│   ├── RVR.AI.RAG.App.RAG/          # Document processing
│   ├── RVR.AI.RAG.App.AI/           # AI providers
│   └── RVR.AI.RAG.App.VectorStore/  # Qdrant client
├── documents/                        # Sample documents
├── docker-compose.yml
└── README.md
```

## AI Providers

| Provider | Models | Embeddings | Cost |
|----------|--------|------------|------|
| OpenAI | GPT-3.5, GPT-4 | text-embedding-ada-002 | Paid |
| Anthropic | Claude 3 | ❌ | Paid |
| Ollama | Llama 2, Mistral | nomic-embed-text | Free |

## RAG Pipeline

1. **Upload Document**: PDF or text file
2. **Text Extraction**: Extract text from document
3. **Chunking**: Split into overlapping chunks
4. **Embedding**: Generate vector embeddings
5. **Vector Storage**: Store in Qdrant
6. **Query**: Search similar chunks for user questions
7. **Context**: Inject relevant chunks into AI prompt

## Token Cost Tracking

Track usage by provider, model, and user:

```
GET /api/usage?from=2024-01-01&to=2024-12-31
```

Response includes:
- Input/Output tokens
- Total cost per model
- Daily breakdown

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| /api/chat | POST | Send chat message |
| /api/documents | POST | Upload document |
| /api/conversations | GET | List conversations |
| /api/usage | GET | Token usage stats |

## Sample Documents

Place PDF files in the `documents/` folder for testing.

## License

MIT License
