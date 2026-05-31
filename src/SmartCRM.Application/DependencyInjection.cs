using Microsoft.Extensions.DependencyInjection;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Application.Services;
using SmartCRM.Application.Common.Abstractions;
using Microsoft.SemanticKernel;

namespace SmartCRM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAiService, ChatService>();
        services.AddSingleton<ITextChunker, SemanticTextChunker>();
        services.AddScoped<ToolTrackingFilter>();
        services.AddScoped<AgentOrchestrator>();
        
        return services;
    }
}
