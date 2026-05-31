using Pgvector;

namespace SmartCRM.Domain.Entities;

public class KnowledgeBase : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
    public string Source { get; set; } = string.Empty; // e.g., "Chính sách chiết khấu 2026.txt"
}
