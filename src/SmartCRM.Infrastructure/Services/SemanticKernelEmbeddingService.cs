using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Embeddings;
using SmartCRM.Application.Common.Interfaces;

namespace SmartCRM.Infrastructure.Services;

public sealed class SemanticKernelEmbeddingService : IEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public SemanticKernelEmbeddingService(ITextEmbeddingGenerationService embeddingService)
    {
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
    }

    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var result = await _embeddingService.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return result.ToArray();
    }
}
