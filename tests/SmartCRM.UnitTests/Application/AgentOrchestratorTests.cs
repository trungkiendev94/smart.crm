using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using SmartCRM.Application.Services;
using Xunit;

namespace SmartCRM.UnitTests.Application;

public class AgentOrchestratorTests
{
    [Fact]
    public async Task ExecuteWorkflowAsync_NoToolsCalled_ReturnsDirectResponse()
    {
        // Arrange
        var mockChatService = new Mock<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        
        var expectedResponse = "Hello, how can I help?";
        var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, expectedResponse);
        
        mockChatService
            .Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(), 
                It.IsAny<PromptExecutionSettings>(), 
                It.IsAny<Kernel>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { chatMessageContent });

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton(mockChatService.Object);
        var kernel = new Kernel(services.BuildServiceProvider());
        
        var toolTracker = new ToolTrackingFilter();
        var orchestrator = new AgentOrchestrator(kernel, toolTracker);

        // Act
        var result = await orchestrator.ExecuteWorkflowAsync(chatHistory);

        // Assert
        Assert.Equal(expectedResponse, result.Response);
        Assert.Empty(result.Tools);
    }
}
