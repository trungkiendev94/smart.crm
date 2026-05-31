using Microsoft.AspNetCore.Mvc;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Domain.Entities;
using SmartCRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System.Threading.Tasks;
using System.Linq;

namespace SmartCRM.WebAPI.Controllers;

public class KnowledgeRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class KnowledgeController : ControllerBase
{
    private readonly SmartCrmDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;

    public KnowledgeController(SmartCrmDbContext dbContext, IEmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> IngestKnowledge([FromBody] KnowledgeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content is required.");
        }

        // Generate embedding using the system's embedding service (gemini-embedding-001)
        var embedding = await _embeddingService.GetEmbeddingAsync(request.Content);

        var knowledge = new KnowledgeBase
        {
            Title = request.Title ?? "Manual Entry",
            Content = request.Content,
            Source = "Web Dashboard",
            Embedding = new Vector(embedding)
        };

        _dbContext.KnowledgeBases.Add(knowledge);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Knowledge successfully saved and vectorized." });
    }

    [HttpGet]
    public async Task<IActionResult> GetKnowledgeList()
    {
        var list = await _dbContext.KnowledgeBases
            .Select(x => new { x.Id, x.Title, x.Content, x.Source, x.CreatedAt })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
            
        return Ok(list);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateKnowledge(Guid id, [FromBody] KnowledgeRequest request)
    {
        var existing = await _dbContext.KnowledgeBases.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Title = request.Title;
        existing.Content = request.Content;
        existing.UpdatedAt = DateTime.UtcNow;

        // Re-vectorize if content changed
        var embedding = await _embeddingService.GetEmbeddingAsync(request.Content);
        existing.Embedding = new Vector(embedding);

        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "Knowledge updated and re-vectorized." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteKnowledge(Guid id)
    {
        var existing = await _dbContext.KnowledgeBases.FindAsync(id);
        if (existing == null) return NotFound();

        _dbContext.KnowledgeBases.Remove(existing);
        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "Knowledge deleted." });
    }
}
