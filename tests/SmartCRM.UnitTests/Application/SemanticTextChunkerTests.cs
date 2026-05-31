using FluentAssertions;
using SmartCRM.Application.Services;
using Xunit;

namespace SmartCRM.UnitTests.Application;

public class SemanticTextChunkerTests
{
    private readonly SemanticTextChunker _sut;

    public SemanticTextChunkerTests()
    {
        _sut = new SemanticTextChunker();
    }

    [Fact]
    public void Chunk_ShouldReturnEmpty_WhenTextIsEmpty()
    {
        // Arrange
        var text = string.Empty;

        // Act
        var result = _sut.Chunk(text, 100);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_ShouldReturnChunks_WhenTextIsProvided()
    {
        // Arrange
        var text = "This is a test sentence. This is another sentence that should be split if it's long enough.";
        var maxTokens = 10;

        // Act
        var result = _sut.Chunk(text, maxTokens);

        // Assert
        result.Should().NotBeEmpty();
        foreach (var chunk in result)
        {
            chunk.Should().NotBeNullOrWhiteSpace();
        }
    }
}
