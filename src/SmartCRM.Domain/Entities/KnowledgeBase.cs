using Pgvector;

namespace SmartCRM.Domain.Entities;

public class KnowledgeBase : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
    public string Source { get; set; } = string.Empty; // e.g., "DiscountPolicy2026.txt"
}
