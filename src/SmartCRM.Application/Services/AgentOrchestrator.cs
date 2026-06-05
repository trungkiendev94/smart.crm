using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SmartCRM.Application.Services;

public class AgentOrchestrator
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ToolTrackingFilter _toolTracker;

    public AgentOrchestrator(Kernel kernel, ToolTrackingFilter toolTracker)
    {
        _kernel = kernel;
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
        _toolTracker = toolTracker;
    }

    public async Task<(string Response, List<string> Tools)> ExecuteWorkflowAsync(ChatHistory history)
    {
        _toolTracker.ExecutedTools.Clear();
        int maxTurns = 3;
        int currentTurn = 0;

        var settings = new PromptExecutionSettings 
        { 
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ExtensionData = new Dictionary<string, object> { { "temperature", 0.1 } }
        };

        while (currentTurn < maxTurns)
        {
            currentTurn++;
            Console.WriteLine($"[AgentOrchestrator] Turn {currentTurn}: Calling LLM...");
            
            ChatMessageContent result;
            try 
            {
                result = await _chatService.GetChatMessageContentAsync(history, settings, _kernel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AgentOrchestrator] CRITICAL ERROR in Turn {currentTurn}: {ex.Message}");
                throw;
            }

            var toolCalls = result.Items.OfType<FunctionCallContent>().ToList();
            if (!toolCalls.Any())
            {
                return (result.Content ?? string.Empty, _toolTracker.ExecutedTools.Distinct().ToList());
            }

            Console.WriteLine($"[AgentOrchestrator] AI requested {toolCalls.Count} tools in turn {currentTurn}.");
            var contextualInfo = new List<string>();
            
            // Add Assistant message with tool calls to history
            history.Add(result);

            foreach (var toolCall in toolCalls)
            {
                try {
                    var toolResult = await toolCall.InvokeAsync(_kernel);
                    var resultString = toolResult?.ToString() ?? "Success (no data returned)";
                    contextualInfo.Add($"RESULT from {toolCall.FunctionName}: {resultString}");
                    
                    // Add tool result back to history for SK to process
                    history.Add(new ChatMessageContent(AuthorRole.Tool, resultString) 
                    { 
                        Items = { new FunctionResultContent(toolCall, toolResult) } 
                    });
                } catch (Exception ex) {
                    contextualInfo.Add($"ERROR in {toolCall.FunctionName}: {ex.Message}");
                    history.Add(new ChatMessageContent(AuthorRole.Tool, $"Error: {ex.Message}") 
                    { 
                        Items = { new FunctionResultContent(toolCall, $"Error: {ex.Message}") } 
                    });
                }
            }
            
            Console.WriteLine($"[AgentOrchestrator] Turn {currentTurn} tools executed. Re-evaluating...");
        }

        Console.WriteLine("[AgentOrchestrator] Reached max turns (3). Returning last response.");
        var finalResult = await _chatService.GetChatMessageContentAsync(history, new PromptExecutionSettings { 
            ExtensionData = new Dictionary<string, object> { { "temperature", 0.1 } } 
        }, _kernel);
        
        return (finalResult.Content ?? string.Empty, _toolTracker.ExecutedTools.Distinct().ToList());
    }
}