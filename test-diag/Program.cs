using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Net.Http;

var endpoint = "http://localhost:5001/v1";
var modelId = "koboldcpp/Qwen3-VL-8B-Instruct-Q4_K_S";

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(modelId, "none", httpClient: new HttpClient { BaseAddress = new Uri(endpoint) });
var kernel = builder.Build();

kernel.Plugins.AddFromObject(new MyTool());

var chat = kernel.GetRequiredService<IChatCompletionService>();
Console.WriteLine("Testing Tool Calling with Local AI...");

var history = new ChatHistory("You are a helpful assistant. Use tools when needed.");
history.AddUserMessage("What is the secret code? Call the tool.");

var settings = new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

try {
    var result = await chat.GetChatMessageContentAsync(history, settings, kernel);
    Console.WriteLine("Final Response: " + result.Content);
} catch (Exception ex) {
    Console.WriteLine("FAILED: " + ex.Message);
}

public class MyTool {
    [KernelFunction]
    [Description("Gets the secret code.")]
    public string GetSecretCode() {
        Console.WriteLine("[Tool] GetSecretCode invoked!");
        return "The secret code is 99999.";
    }
}
