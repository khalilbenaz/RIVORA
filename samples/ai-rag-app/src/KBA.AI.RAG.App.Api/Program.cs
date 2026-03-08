using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using KBA.AI.RAG.App.Infrastructure;
using KBA.AI.RAG.App.Domain.Entities;
using KBA.AI.RAG.App.Application.DTOs;
using KBA.AI.RAG.App.RAG;
using KBA.AI.RAG.App.AI;
using KBA.AI.RAG.App.VectorStore;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5001", "https://localhost:5001")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KBA AI RAG API", Version = "v1" });
});

var app = builder.Build();

app.UseCors();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Chat endpoint
app.MapPost("/api/chat", async (ChatRequest request, AiRagDbContext db, IAiProvider aiProvider, IVectorStore vectorStore, IRagPipeline rag) =>
{
    var startTime = DateTime.UtcNow;
    
    // Get or create conversation
    Conversation? conversation = null;
    if (request.ConversationId.HasValue)
    {
        conversation = await db.Conversations.FindAsync(request.ConversationId.Value);
    }
    
    if (conversation == null)
    {
        conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Title = request.Message.Length > 50 ? request.Message[..50] + "..." : request.Message,
            Provider = request.Provider,
            Model = request.Model,
            CreatedAt = DateTime.UtcNow
        };
        db.Conversations.Add(conversation);
    }

    // Add user message
    var userMessage = new ChatMessage
    {
        Id = Guid.NewGuid(),
        Role = "User",
        Content = request.Message,
        ConversationId = conversation.Id,
        CreatedAt = DateTime.UtcNow
    };
    db.ChatMessages.Add(userMessage);

    // RAG: Search for relevant documents
    List<SourceDocument> sources = new();
    if (request.UseRAG)
    {
        try
        {
            // Generate embedding for query
            var embedding = await aiProvider.EmbedAsync(request.Message);
            
            // Search vector store
            var results = await vectorStore.SearchAsync("documents", embedding.Embedding, limit: 5);
            
            sources = results.Select(r => new SourceDocument
            {
                Id = Guid.Parse(r.Metadata.GetValueOrDefault("document_id", Guid.Empty.ToString())),
                Title = r.Metadata.GetValueOrDefault("document_title", "Unknown"),
                Content = r.Content,
                RelevanceScore = r.Score
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Warning($"RAG search failed: {ex.Message}");
        }
    }

    // Build messages with context
    var messages = new List<Message>
    {
        new Message { Role = "system", Content = "You are a helpful AI assistant. Use the provided context to answer questions." }
    };

    if (sources.Any())
    {
        var context = string.Join("\n\n---\n\n", sources.Select(s => $"[Source: {s.Title}]\n{s.Content}"));
        messages.Add(new Message { Role = "system", Content = $"Context from documents:\n{context}" });
    }

    // Add conversation history
    var history = db.ChatMessages
        .Where(m => m.ConversationId == conversation.Id)
        .OrderBy(m => m.CreatedAt)
        .TakeLast(10);
    
    foreach (var msg in history)
    {
        messages.Add(new Message { Role = msg.Role == "User" ? "user" : "assistant", Content = msg.Content });
    }

    messages.Add(new Message { Role = "user", Content = request.Message });

    // Call AI provider
    var aiRequest = new AiRequest
    {
        Messages = messages,
        Model = request.Model,
        Temperature = request.Temperature,
        MaxTokens = request.MaxTokens
    };

    var aiResponse = await aiProvider.CompleteAsync(aiRequest);

    // Add assistant response
    var assistantMessage = new ChatMessage
    {
        Id = Guid.NewGuid(),
        Role = "Assistant",
        Content = aiResponse.Content,
        ConversationId = conversation.Id,
        Provider = request.Provider,
        Model = request.Model,
        TokenCount = aiResponse.TotalTokens,
        Cost = aiProvider.CalculateCost(aiResponse.InputTokens, aiResponse.OutputTokens),
        Latency = aiResponse.Latency,
        CreatedAt = DateTime.UtcNow
    };
    db.ChatMessages.Add(assistantMessage);

    // Update conversation stats
    conversation.TotalTokens += aiResponse.TotalTokens;
    conversation.TotalCost += assistantMessage.Cost;

    // Track token usage
    db.TokenUsages.Add(new TokenUsage
    {
        Id = Guid.NewGuid(),
        UserId = conversation.UserId,
        Provider = request.Provider,
        Model = request.Model,
        InputTokens = aiResponse.InputTokens,
        OutputTokens = aiResponse.OutputTokens,
        TotalTokens = aiResponse.TotalTokens,
        Cost = assistantMessage.Cost,
        UsageDate = DateTime.UtcNow
    });

    await db.SaveChangesAsync();

    return Results.Ok(new ChatResponse
    {
        Message = aiResponse.Content,
        ConversationId = conversation.Id,
        MessageId = assistantMessage.Id,
        Provider = request.Provider,
        Model = request.Model,
        TokenCount = aiResponse.TotalTokens,
        Cost = assistantMessage.Cost,
        Latency = DateTime.UtcNow - startTime,
        Sources = sources
    });
})
.WithName("Chat")
.WithOpenApi();

// Document upload endpoint
app.MapPost("/api/documents", async (IFormFile file, AiRagDbContext db, IRagPipeline rag, IVectorStore vectorStore, IAiProvider aiProvider) =>
{
    var document = new Document
    {
        Id = Guid.NewGuid(),
        Title = Path.GetFileNameWithoutExtension(file.FileName),
        FileName = file.FileName,
        FileType = Path.GetExtension(file.FileName).ToLowerInvariant(),
        FileSize = file.Length,
        Status = DocumentStatus.Processing,
        CreatedAt = DateTime.UtcNow
    };

    // Read file content
    using var stream = file.OpenReadStream();
    using var reader = new StreamReader(stream);
    document.Content = await reader.ReadToEndAsync();

    db.Documents.Add(document);
    await db.SaveChangesAsync();

    try
    {
        // Process document
        var chunks = await rag.ProcessDocumentAsync(document);
        
        // Generate embeddings and store in vector DB
        foreach (var chunk in chunks)
        {
            var embedding = await aiProvider.EmbedAsync(chunk.Content);
            await vectorStore.UpsertAsync("documents", chunk, embedding.Embedding);
        }

        document.Status = DocumentStatus.Indexed;
        await db.SaveChangesAsync();

        return Results.Ok(new DocumentUploadResult
        {
            DocumentId = document.Id,
            FileName = document.FileName,
            Success = true,
            ChunkCount = chunks.Count
        });
    }
    catch (Exception ex)
    {
        document.Status = DocumentStatus.Failed;
        document.ErrorMessage = ex.Message;
        await db.SaveChangesAsync();

        return Results.Ok(new DocumentUploadResult
        {
            DocumentId = document.Id,
            FileName = document.FileName,
            Success = false,
            ErrorMessage = ex.Message
        });
    }
})
.DisableAntiforgery()
.WithName("UploadDocument")
.WithOpenApi();

// Get conversations endpoint
app.MapGet("/api/conversations", async (AiRagDbContext db) =>
{
    var conversations = await db.Conversations
        .OrderByDescending(c => c.CreatedAt)
        .Select(c => new ConversationDto
        {
            Id = c.Id,
            Title = c.Title,
            CreatedAt = c.CreatedAt,
            MessageCount = c.Messages.Count,
            TotalTokens = c.TotalTokens,
            TotalCost = c.TotalCost
        })
        .ToListAsync();
    return Results.Ok(conversations);
})
.WithName("GetConversations")
.WithOpenApi();

// Get token usage endpoint
app.MapGet("/api/usage", async (AiRagDbContext db, DateTime? from, DateTime? to) =>
{
    var query = db.TokenUsages.AsQueryable();
    
    if (from.HasValue)
        query = query.Where(t => t.UsageDate >= from.Value);
    if (to.HasValue)
        query = query.Where(t => t.UsageDate <= to.Value);

    var usage = await query
        .GroupBy(t => new { t.Provider, t.Model, Date = t.UsageDate.Date })
        .Select(g => new TokenUsageDto
        {
            Date = g.Key.Date,
            Provider = g.Key.Provider,
            Model = g.Key.Model,
            InputTokens = g.Sum(t => t.InputTokens),
            OutputTokens = g.Sum(t => t.OutputTokens),
            TotalTokens = g.Sum(t => t.TotalTokens),
            Cost = g.Sum(t => t.Cost)
        })
        .OrderByDescending(t => t.Date)
        .ToListAsync();

    return Results.Ok(usage);
})
.WithName("GetTokenUsage")
.WithOpenApi();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AiRagDbContext>();
    await db.Database.MigrateAsync();
    
    // Ensure vector collection exists
    var vectorStore = scope.ServiceProvider.GetRequiredService<IVectorStore>();
    try
    {
        await vectorStore.CreateCollectionAsync("documents", 1536);
    }
    catch { /* Collection may already exist */ }
}

Log.Information("Starting KBA AI RAG API");
app.Run();
