using System.Collections.Generic;

namespace SmartCRM.Application.Common.Abstractions;

/// <summary>
/// Strategy pattern for text chunking.
/// </summary>
public interface ITextChunker
{
    IEnumerable<string> Chunk(string text, int maxTokensPerChunk);
}
