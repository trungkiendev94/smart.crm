using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SmartCRM.Application.Common.Interfaces;

namespace SmartCRM.Application.Services;

public class LlmRerankService : IRerankService
{
    private readonly IChatCompletionService _chatService;
    private readonly ISystemSettingsService _settingsService;

    public LlmRerankService(IChatCompletionService chatService, ISystemSettingsService settingsService)
    {
        _chatService = chatService;
        _settingsService = settingsService;
    }

    public async Task<List<string>> RerankAsync(string query, List<string> documents, int topK = 3)
    {
        if (documents == null || !documents.Any())
        {
            return new List<string>();
        }

        if (documents.Count <= 1)
        {
            return documents;
        }

        Console.WriteLine($"[LlmRerankService] Reranking {documents.Count} documents for query: {query}");

        var promptTemplate = await _settingsService.GetSettingAsync("RerankPrompt", 
            "Analyze the QUERY and each document. Select up to {topK} most useful documents. Return as JSON array of IDs. QUERY: {query} DOCUMENTS: {documents}");

        var prompt = promptTemplate
            .Replace("{query}", query)
            .Replace("{topK}", topK.ToString())
            .Replace("{documents}", string.Join("\n\n", documents.Select((doc, index) => $"[ID: {index}]\n{doc}")));

        try
        {
            var result = await _chatService.GetChatMessageContentAsync(prompt);
            var content = result.Content;
            Console.WriteLine($"[LlmRerankService] LLM Raw Response: {content}");

            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("[LlmRerankService] Empty response from LLM, falling back to original order.");
                return documents.Take(topK).ToList();
            }

            var match = Regex.Match(content, @"\[[\d,\s]+\]");
            if (match.Success)
            {
                var ids = JsonSerializer.Deserialize<List<int>>(match.Value);
                if (ids != null)
                {
                    var reranked = ids
                        .Where(id => id >= 0 && id < documents.Count)
                        .Select(id => documents[id])
                        .Distinct()
                        .Take(topK)
                        .ToList();

                    if (reranked.Any())
                    {
                        Console.WriteLine($"[LlmRerankService] Successfully reranked and picked {reranked.Count} docs.");
                        return reranked;
                    }
                }
            }
            Console.WriteLine("[LlmRerankService] Could not parse IDs from LLM response, falling back to original order.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LlmRerankService] Error during reranking: {ex.Message}");
        }

        // Fallback to original order if reranking fails
        return documents.Take(topK).ToList();
    }
}
