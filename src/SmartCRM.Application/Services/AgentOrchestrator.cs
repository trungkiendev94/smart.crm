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
        
        var settings = new PromptExecutionSettings 
        { 
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ExtensionData = new Dictionary<string, object> { { "temperature", 0.1 } }
        };
        
        Console.WriteLine("[AgentOrchestrator] Turn 1: Calling LLM to decide tools...");
        var result = await _chatService.GetChatMessageContentAsync(history, settings, _kernel);

        var toolCalls = result.Items.OfType<FunctionCallContent>().ToList();
        if (toolCalls.Any())
        {
            Console.WriteLine($"[AgentOrchestrator] AI requested {toolCalls.Count} tools.");
            var contextualInfo = new List<string>();
            foreach (var toolCall in toolCalls)
            {
                try {
                    var toolResult = await toolCall.InvokeAsync(_kernel);
                    contextualInfo.Add($"DATA from {toolCall.FunctionName}: {toolResult?.ToString()}");
                } catch (Exception ex) {
                    contextualInfo.Add($"ERROR in {toolCall.FunctionName}: {ex.Message}");
                }
            }

            history.AddSystemMessage("INTERNAL RECORDS:\n" + string.Join("\n", contextualInfo) + "\n\nRemember to ONLY output valid JSON.");
            
            Console.WriteLine("[AgentOrchestrator] Turn 2: Synthesizing final response...");
            var finalResult = await _chatService.GetChatMessageContentAsync(history, new PromptExecutionSettings { 
                ExtensionData = new Dictionary<string, object> { { "temperature", 0.1 } } 
            }, _kernel);
            
            return (finalResult.Content ?? string.Empty, _toolTracker.ExecutedTools.Distinct().ToList());
        }

        return (result.Content ?? string.Empty, _toolTracker.ExecutedTools.Distinct().ToList());
    }
}