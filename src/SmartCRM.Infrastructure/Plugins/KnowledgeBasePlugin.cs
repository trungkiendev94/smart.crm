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
    private readonly IRerankService _rerankService;

    public KnowledgeBasePlugin(SmartCrmDbContext dbContext, IEmbeddingService embeddingService, IRerankService rerankService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _rerankService = rerankService;
    }

    [KernelFunction]
    [Description("Searches the official internal knowledge base for company-specific information, products, discount policies, or support documents. Use this when the user's question involves internal company details.")]
    public async Task<string> SearchKnowledgeBase(
        [Description("The specific search query extracted from the user's question")] string query)
    {
        try 
        {
            Console.WriteLine($"[KnowledgeBasePlugin] Searching for: {query}");
            // 1. Generate embedding for the query
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query);
            var vector = new Vector(queryEmbedding);

            // 2. Perform Vector Search (Cosine Similarity) via EF Core
            // Increase limit to get more candidates for reranking
            var candidates = await _dbContext.KnowledgeBases
                .OrderBy(x => x.Embedding!.L2Distance(vector))
                .Take(10)
                .Select(x => x.Content)
                .ToListAsync();

            Console.WriteLine($"[KnowledgeBasePlugin] Found {candidates.Count} candidates. Starting reranking...");

            if (!candidates.Any())
            {
                Console.WriteLine("[KnowledgeBasePlugin] No candidates found in database.");
                return "No relevant information found in the official knowledge base.";
            }

            // 3. Rerank candidates to get the most relevant top 3
            var results = await _rerankService.RerankAsync(query, candidates, topK: 3);

            Console.WriteLine($"[KnowledgeBasePlugin] Reranking complete. Returning {results.Count} results.");
            return string.Join("\n---\n", results);
            }
            catch (Exception ex)
            {
            Console.WriteLine($"[KnowledgeBasePlugin] ERROR: {ex.Message}");
            return $"Error searching internal knowledge base: {ex.Message}";
            }

    }
}
