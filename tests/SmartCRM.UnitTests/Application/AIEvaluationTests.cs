using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace SmartCRM.UnitTests.Application;

public class AIEvaluationTests
{
    /// <summary>
    /// LLM-as-a-judge approach: We use an LLM (here mocked, but in practice a real LLM) 
    /// to evaluate if a generated answer is correct and grounded in the provided context.
    /// </summary>
    [Fact]
    public async Task Evaluate_RagResponse_ShouldBeGroundedAndCorrect()
    {
        // Arrange
        string question = "NPO có những chiến dịch nào?";
        string context = "Chiến dịch A (1000/2000), Chiến dịch B (500/1000)";
        string generatedAnswer = "NPO hiện có Chiến dịch A và Chiến dịch B đang hoạt động.";
        
        var evaluationPrompt = $@"
You are an expert evaluator. Evaluate the following generated answer based on the context.
Question: {question}
Context: {context}
Generated Answer: {generatedAnswer}

Is the generated answer strictly derived from the context and accurate? 
Respond with 'YES' or 'NO'.";

        var mockChatService = new Mock<IChatCompletionService>();
        var expectedEvaluationResult = "YES";
        
        mockChatService
            .Setup(c => c.GetChatMessageContentsAsync(
                It.Is<ChatHistory>(h => h[0].Content.Contains(generatedAnswer)), 
                It.IsAny<PromptExecutionSettings>(), 
                It.IsAny<Kernel>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { new ChatMessageContent(AuthorRole.Assistant, expectedEvaluationResult) });

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton(mockChatService.Object);
        var kernel = new Kernel(services.BuildServiceProvider());

        // Act
        var evaluatorHistory = new ChatHistory(evaluationPrompt);
        var evalResult = await mockChatService.Object.GetChatMessageContentAsync(evaluatorHistory, null, kernel);

        // Assert
        Assert.Contains("YES", evalResult.Content);
    }
}
