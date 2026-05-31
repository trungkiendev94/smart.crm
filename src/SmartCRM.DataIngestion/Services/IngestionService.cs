using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartCRM.Application.Common.Abstractions;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Domain.Entities;
using SmartCRM.Infrastructure.Persistence;
using Pgvector;

namespace SmartCRM.DataIngestion.Services;

public sealed class IngestionService
{
    private readonly SmartCrmDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly ITextChunker _textChunker;
    private readonly ILogger<IngestionService> _logger;

    public IngestionService(
        SmartCrmDbContext dbContext,
        IEmbeddingService embeddingService,
        ITextChunker textChunker,
        ILogger<IngestionService> logger)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _textChunker = textChunker;
        _logger = logger;
    }

    public async Task IngestFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting ingestion for file: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            _logger.LogError("File not found: {FilePath}", filePath);
            return;
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var title = Path.GetFileNameWithoutExtension(filePath);
        
        var chunks = _textChunker.Chunk(content, maxTokensPerChunk: 500).ToList();
        _logger.LogInformation("Split file into {Count} chunks", chunks.Count);

        foreach (var (chunk, index) in chunks.Select((c, i) => (c, i)))
        {
            _logger.LogInformation("Processing chunk {Index}/{Count}", index + 1, chunks.Count);
            
            var embedding = await _embeddingService.GetEmbeddingAsync(chunk, cancellationToken);
            
            var knowledge = new KnowledgeBase
            {
                Title = $"{title} - Part {index + 1}",
                Content = chunk,
                Source = Path.GetFileName(filePath),
                Embedding = new Vector(embedding)
            };

            _dbContext.KnowledgeBases.Add(knowledge);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully ingested {FilePath}", filePath);
    }
}
