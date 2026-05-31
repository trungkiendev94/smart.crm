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

# RULES
1. Match user language (Vietnamese/English).
2. Use emojis ✨.
{JsonSchemaInstruction}";
        
        history.AddSystemMessage(systemInstruction);
        history.AddUserMessage(prompt);
        return history;
    }

    private string ParseJsonResponse(string? content)
    {
        if (string.IsNullOrEmpty(content)) return "Tôi đã xử lý yêu cầu thành công! ✨";

        try 
        {
            var match = Regex.Match(content, @"\{.*\}", RegexOptions.Singleline);
            if (match.Success)
            {
                using var doc = JsonDocument.Parse(match.Value);
                if (doc.RootElement.TryGetProperty("reply", out var replyElement))
                {
                    return replyElement.GetString() ?? "Tôi đã ghi nhận thông tin. 😊";
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
            return new ChatResponse("Hệ thống gặp sự cố kết nối. Bạn thử lại sau nhé! 🛠️", new System.Collections.Generic.List<string>());
        }
    }

    public async Task<string> ExecuteAgentTaskAsync(string taskDescription) => "";
}
