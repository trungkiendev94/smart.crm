using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SmartCRM.Infrastructure;
using SmartCRM.Application.Common.Abstractions;
using SmartCRM.Application.Common.Interfaces;
using SmartCRM.Application.Services;
using SmartCRM.Infrastructure.Services;
using SmartCRM.DataIngestion.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

// Setup Host
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Infrastructure
        services.AddInfrastructure(context.Configuration);

        // Application Services
        services.AddSingleton<ITextChunker, SemanticTextChunker>();
        
        // AI Services (Semantic Kernel)
        var aiSettings = context.Configuration.GetSection("AiSettings");
        var apiKey = aiSettings["ApiKey"] ?? string.Empty;
        var builder = Kernel.CreateBuilder();
        
        // Add Google Gemini Embedding Service (Free)
        builder.AddGoogleAIEmbeddingGeneration("gemini-embedding-001", apiKey);
        
        var kernel = builder.Build();
        services.AddSingleton(kernel);
        services.AddSingleton<ITextEmbeddingGenerationService>(
            kernel.GetRequiredService<ITextEmbeddingGenerationService>());
            
        services.AddSingleton<IEmbeddingService, SemanticKernelEmbeddingService>();
        services.AddScoped<IngestionService>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var ingestionService = host.Services.GetRequiredService<IngestionService>();

logger.LogInformation("SmartCRM Data Ingestion Tool Started.");

// Ingest multiple knowledge files
string[] filesToIngest = { "ChinhSachChietKhau2026.txt", "Products.txt" };

foreach (var fileName in filesToIngest)
{
    if (File.Exists(fileName))
    {
        try 
        {
            await ingestionService.IngestFileAsync(fileName);
            logger.LogInformation($"Successfully ingested: {fileName}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error ingesting {fileName}");
        }
    }
    else
    {
        logger.LogWarning($"File not found: {fileName}");
    }
}

logger.LogInformation("Ingestion process completed.");

await host.RunAsync();
