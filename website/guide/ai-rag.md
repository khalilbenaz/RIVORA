# AI & RAG

## Module RAG

Le module RAG (Retrieval-Augmented Generation) permet d'integrer l'IA dans votre application :

```csharp
builder.Services.AddRvrAI(options =>
{
    options.Provider = AIProvider.OpenAI; // ou Claude, Ollama
    options.ApiKey = builder.Configuration["AI:ApiKey"];
    options.EmbeddingModel = "text-embedding-3-small";
    options.ChatModel = "gpt-4o";
});
```

### Pipeline RAG

1. **Ingestion** : charger des documents (PDF, texte, markdown)
2. **Chunking** : decouper en segments de taille optimale
3. **Embedding** : vectoriser via le modele choisi
4. **Stockage** : sauvegarder dans le vector store
5. **Recherche** : similarity search sur requete utilisateur
6. **Generation** : enrichir le prompt LLM avec le contexte

```csharp
// Ingestion
await _ragService.IngestAsync(new Document
{
    Content = pdfText,
    Metadata = new { source = "manual.pdf", category = "docs" }
});

// Recherche + Chat
var response = await _ragService.ChatAsync(
    "Comment configurer le multi-tenancy ?",
    options: new ChatOptions { MaxTokens = 500 }
);
```

## NL Query Builder

Transforme du langage naturel en requetes LINQ (FR/EN) :

```csharp
var query = await _nlQueryBuilder.BuildAsync<Product>(
    "produits actifs avec prix superieur a 100 tries par nom"
);
// Genere : Products.Where(p => p.Status == Active && p.Price > 100).OrderBy(p => p.Name)
```

## CLI AI Review

```bash
rvr ai review --all                    # Tous les analyzers
rvr ai review --architecture           # Clean Architecture conformance
rvr ai review --ddd                    # DDD anti-patterns
rvr ai review --performance            # N+1, missing async, EF anti-patterns
rvr ai review --security               # Vulnerabilites OWASP
rvr ai review --provider ollama        # Mode offline
rvr ai review --output sarif           # Integration CI/CD
```

### Backends LLM supportes

| Provider | Modeles | Usage |
|----------|---------|-------|
| OpenAI | GPT-4o, text-embedding-3 | Cloud, haute qualite |
| Claude | Claude 3.5 Sonnet | Cloud, analyse code |
| Ollama | Llama, Mistral, CodeLlama | Local, offline |
