using SmartCRM.Infrastructure;
using SmartCRM.Application;
using SmartCRM.Infrastructure.Persistence;
using SmartCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("SmartCRM.WebAPI"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddConsoleExporter();
    });

// Add Logging with OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.AddConsoleExporter();
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vue default port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register Custom Services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Automatic Database Initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SmartCrmDbContext>();
        // Professional approach: Ensure database and tables are created
        await context.Database.EnsureCreatedAsync();

        // Seed Default System Settings if missing
        if (!await context.SystemSettings.AnyAsync(x => x.Key == "AgentInstructions"))
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Key = "AgentInstructions",
                Value = "# SMARTCRM AI ASSISTANT INSTRUCTION\n\n## ROLE\nYou are a professional CRM Consultant. You provide accurate info and help manage leads.\n\n## LANGUAGE RULE\n- **ENGLISH ONLY**: Always respond in English. If the user asks in another language, respond in English politely."
            });
            await context.SaveChangesAsync();
        }


        Console.WriteLine("Database and settings initialized successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// Configure the HTTP request pipeline.
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection(); // Disabled for local development to avoid SSL issues with Vue
app.UseAuthorization();
app.MapControllers();

app.Run();
