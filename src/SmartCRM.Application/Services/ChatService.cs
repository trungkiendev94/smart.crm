using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel.ChatCompletion;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Application.Models;

namespace SmartCRM.Application.Services;

public sealed class ChatService : IAiService
{
    private readonly AgentOrchestrator _orchestrator;
    private readonly ISystemSettingsService _settingsService;

    private const string JsonSchemaInstruction = @"
RESPONSE FORMAT (STRICT):
Respond ONLY with a valid JSON object. No other text.
Schema: { ""reply"": ""your_response_here"" }";

    public ChatService(AgentOrchestrator orchestrator, ISystemSettingsService settingsService)
    {
        _orchestrator = orchestrator;
        _settingsService = settingsService;
    }

    private async Task<ChatHistory> CreateHistoryAsync(string prompt)
    {
        var history = new ChatHistory();
        string instructions = await _settingsService.GetSettingAsync("AgentInstructions", 
            "You are a professional SmartCRM Assistant.");
        
        var systemInstruction = $@"
# IDENTITY
{instructions}

# MANDATORY LANGUAGE RULE
- You MUST respond ONLY in Vietnamese or English.
- NEVER use Chinese characters (Hanzi/Chu Han) under any circumstances.
- If the user speaks Vietnamese, respond in Vietnamese.
- If the user speaks English, respond in English.
- DO NOT use Chinese particles like 'ne', 'ma', etc.

# DATA INTEGRITY RULES
1. NEVER assume or hallucinate customer data (emails, names, etc.).
2. If the user mentions a name, you MUST use `SearchCustomer` to find their real email before using tools like `SendEmailMarketing`.
3. If multiple customers are found, ask the user to clarify which one they mean.
4. If no customer is found, inform the user and ask if they want to create a new lead.

# STYLE
1. Maintain a professional tone.
{JsonSchemaInstruction}";
        
        history.AddSystemMessage(systemInstruction);
        history.AddUserMessage(prompt);
        return history;
    }

    private string ParseJsonResponse(string? content)
    {
        if (string.IsNullOrEmpty(content)) return "Success";

        try 
        {
            var match = Regex.Match(content, @"\{.*\}", RegexOptions.Singleline);
            if (match.Success)
            {
                using var doc = JsonDocument.Parse(match.Value);
                if (doc.RootElement.TryGetProperty("reply", out var replyElement))
                {
                    return replyElement.GetString() ?? "Information recorded successfully.";
                }
            }
        }
        catch 
        {
            Console.WriteLine("[ChatService] JSON Parsing failed, falling back to raw content.");
        }

        return content.Trim();
    }

    public async Task<ChatResponse> GetResponseAsync(string prompt)
    {
        try 
        {
            var history = await CreateHistoryAsync(prompt);
            var (responseContent, tools) = await _orchestrator.ExecuteWorkflowAsync(history);
            
            return new ChatResponse(ParseJsonResponse(responseContent), tools);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatService] Global Error: {ex.Message}");
            return new ChatResponse("The system is experiencing connection issues. Please try again later.", new System.Collections.Generic.List<string>());
        }
    }

    public async Task<string> ExecuteAgentTaskAsync(string taskDescription) => "";
}
