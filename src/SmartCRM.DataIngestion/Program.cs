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
        // Infrastructure handles all AI and DB registrations
        services.AddInfrastructure(context.Configuration);

        // Application Services
        services.AddSingleton<ITextChunker, SemanticTextChunker>();
        services.AddScoped<IngestionService>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
using var scope = host.Services.CreateScope();
var ingestionService = scope.ServiceProvider.GetRequiredService<IngestionService>();

logger.LogInformation("SmartCRM Data Ingestion Tool Started.");

// Ingest multiple knowledge files
string[] filesToIngest = { 
    Path.Combine(AppContext.BaseDirectory, "ChinhSachChietKhau2026.txt"), 
    Path.Combine(AppContext.BaseDirectory, "Products.txt"),
    Path.Combine(AppContext.BaseDirectory, "DiscountPolicy2026.txt")
};

// Also check root project directory if not in bin
if (!File.Exists(filesToIngest[0]))
{
    filesToIngest = new[] { "ChinhSachChietKhau2026.txt", "Products.txt", "DiscountPolicy2026.txt" };
}

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
// Do not use host.RunAsync() for a CLI tool that finishes its task
