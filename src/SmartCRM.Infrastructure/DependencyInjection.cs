using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SmartCRM.Infrastructure.Persistence;
using SmartCRM.Infrastructure.Plugins;
using SmartCRM.Infrastructure.Services;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Application.Services;
using MassTransit;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.Google;

namespace SmartCRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureMassTransit = null)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SmartCrmDbContext>(options =>
            options.UseNpgsql(connectionString, x => x.UseVector()));

        // MassTransit Configuration
        services.AddMassTransit(x =>
        {
            configureMassTransit?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ") ?? "localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });

        // AI Configuration
        var aiSettings = configuration.GetSection("AiSettings");
        var apiKey = aiSettings["ApiKey"] ?? "none";
        var modelId = aiSettings["ModelId"] ?? "local";
        var endpoint = aiSettings["Endpoint"] ?? "http://localhost:5001/v1";

        // 1. Register AI Services
        var httpClient = new HttpClient { BaseAddress = new Uri(endpoint), Timeout = TimeSpan.FromMinutes(10) };
        services.AddSingleton(httpClient);

        // Standard SK registration
        services.AddKernel()
                .AddOpenAIChatCompletion(modelId, apiKey, httpClient: httpClient)
                .AddOpenAITextEmbeddingGeneration("all-minilm", apiKey, httpClient: httpClient);

        // 2. Register internal services
        services.AddScoped<ISystemSettingsService, SystemSettingsService>();
        services.AddScoped<IEmbeddingService, SemanticKernelEmbeddingService>();
        services.AddScoped<IRerankService, LlmRerankService>();

        // 3. Register Plugins and Filters
        services.AddScoped<ToolTrackingFilter>();
        services.AddScoped<GuardrailFilter>();
        services.AddScoped<PiiFilter>();
        services.AddScoped<KnowledgeBasePlugin>();
        services.AddScoped<CrmPlugin>();
        services.AddScoped<NpoPlugin>();

        // 4. Register the Kernel instance
        services.AddScoped(sp => 
        {
            var kernel = new Kernel(sp);
            kernel.FunctionInvocationFilters.Add(sp.GetRequiredService<ToolTrackingFilter>());
            kernel.FunctionInvocationFilters.Add(sp.GetRequiredService<GuardrailFilter>());
            kernel.PromptRenderFilters.Add(sp.GetRequiredService<PiiFilter>());
            kernel.Plugins.AddFromObject(sp.GetRequiredService<KnowledgeBasePlugin>());
            kernel.Plugins.AddFromObject(sp.GetRequiredService<CrmPlugin>());
            kernel.Plugins.AddFromObject(sp.GetRequiredService<NpoPlugin>());
            return kernel;
        });

        return services;
    }
}
