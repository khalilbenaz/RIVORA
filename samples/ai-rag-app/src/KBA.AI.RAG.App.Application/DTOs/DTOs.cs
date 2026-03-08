namespace KBA.AI.RAG.App.Application.DTOs;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
    public string Provider { get; set; } = "OpenAI";
    public string Model { get; set; } = "gpt-3.5-turbo";
    public bool UseRAG { get; set; } = true;
    public int MaxTokens { get; set; } = 1024;
    public double Temperature { get; set; } = 0.7;
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public decimal Cost { get; set; }
    public TimeSpan Latency { get; set; }
    public List<SourceDocument> Sources { get; set; } = new();
}

public class SourceDocument
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
}

public class DocumentUploadResult
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ChunkCount { get; set; }
}

public class TokenUsageDto
{
    public DateTime Date { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public decimal Cost { get; set; }
}

public class ProviderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Dictionary<string, string> Models { get; set; } = new();
}

public class ConversationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MessageCount { get; set; }
    public int TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
}
