# AI RAG App

Application de Retrieval-Augmented Generation utilisant le module AI de RIVORA.

## Modules utilises

- **RVR.Framework.AI** : RAG, vector store, embeddings
- **RVR.Framework.AI.Guardrails** : Prompt injection detection, PII masking
- **RVR.Framework.Security** : JWT, RBAC

## Entites

| Entite | Description |
|--------|-------------|
| Document | Document ingere (titre, contenu, chunks) |
| Conversation | Session de chat avec historique |
| Message | Message dans une conversation |

## Fonctionnalites

- Ingestion de documents (PDF, TXT, MD)
- Chunking avec sliding window
- Vector search (cosine similarity)
- Chat avec contexte RAG
- Guardrails (injection detection, PII masking)

## Demarrage

```bash
cd samples/ai-rag-app
docker compose up -d    # Qdrant + PostgreSQL + Ollama
dotnet run --project src/RVR.AI.RAG.App.Api
```

## Architecture

```
src/
├── RVR.AI.RAG.App.AI/         # Providers IA (OpenAI, Ollama)
├── RVR.AI.RAG.App.Api/        # API REST
├── RVR.AI.RAG.App.Application/ # Services, DTOs
└── RVR.AI.RAG.App.Blazor/     # Interface Blazor
```
