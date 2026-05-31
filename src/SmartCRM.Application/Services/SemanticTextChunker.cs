using System;
using System.Collections.Generic;
using Microsoft.SemanticKernel.Text;

namespace SmartCRM.Application.Services;

using SmartCRM.Application.Common.Abstractions;

#pragma warning disable SKEXP0050 // Microsoft.SemanticKernel.Text is experimental
public sealed class SemanticTextChunker : ITextChunker
{
    public IEnumerable<string> Chunk(string text, int maxTokensPerChunk)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<string>();

        // High-level implementation using Semantic Kernel's TextChunker
        var lines = TextChunker.SplitPlainTextLines(text, maxTokensPerChunk);
        return TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerChunk);
    }
}
#pragma warning restore SKEXP0050
