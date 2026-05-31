namespace SmartCRM.Application.Models;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    public ChatResponse(string reply, List<string>? executedTools = null)
    {
        Reply = reply;
        ExecutedTools = executedTools ?? new List<string>();
    }
    public string Reply { get; set; } = string.Empty;
    public List<string> ExecutedTools { get; set; } = new();
}
