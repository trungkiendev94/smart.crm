using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Infrastructure.Persistence;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace SmartCRM.Infrastructure.Plugins;

/// <summary>
/// A Semantic Kernel Plugin that allows the AI to search the internal knowledge base.
/// </summary>
public sealed class KnowledgeBasePlugin
{
    private readonly SmartCrmDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;

    public KnowledgeBasePlugin(SmartCrmDbContext dbContext, IEmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
    }

    [KernelFunction]
    [Description("Searches the official internal knowledge base for company-specific information, products, discount policies, or support documents. Use this when the user's question involves internal company details.")]
    public async Task<string> SearchKnowledgeBase(
        [Description("The specific search query extracted from the user's question")] string query)
    {
        Console.WriteLine($"[KnowledgeBasePlugin] Searching for: {query}");
        // 1. Generate embedding for the query
        var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query);
        var vector = new Vector(queryEmbedding);

        // 2. Perform Vector Search (Cosine Similarity) via EF Core
        var results = await _dbContext.KnowledgeBases
            .OrderBy(x => x.Embedding!.L2Distance(vector))
            .Take(3)
            .Select(x => x.Content)
            .ToListAsync();

        Console.WriteLine($"[KnowledgeBasePlugin] Found {results.Count} results.");

        if (!results.Any())
        {
            return "No relevant information found in the knowledge base.";
        }

        return string.Join("\n---\n", results);
    }
}
