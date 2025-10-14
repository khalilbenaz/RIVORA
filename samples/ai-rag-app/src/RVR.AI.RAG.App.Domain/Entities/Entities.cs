namespace RVR.AI.RAG.App.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Document : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? VectorId { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public string? ErrorMessage { get; set; }
}

public enum DocumentStatus
{
    Pending,
    Processing,
    Indexed,
    Failed
}

public class ChatMessage : BaseEntity
{
    public string Role { get; set; } = string.Empty; // User, Assistant, System
    public string Content { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
    public string? Provider { get; set; }
    public string? Model { get; set; }
    public int TokenCount { get; set; }
    public decimal Cost { get; set; }
    public TimeSpan Latency { get; set; }
    public List<string> SourceDocumentIds { get; set; } = new();
}

public class Conversation : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
}

public class TokenUsage : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public decimal Cost { get; set; }
    public DateTime UsageDate { get; set; }
}

public class AiProvider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ProviderType Type { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string? Endpoint { get; set; }
    public bool IsActive { get; set; } = true;
    public Dictionary<string, string> Models { get; set; } = new();
}

public enum ProviderType
{
    OpenAI,
    AzureOpenAI,
    Anthropic,
    Ollama,
    Local
}
