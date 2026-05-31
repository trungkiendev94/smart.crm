using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SmartCRM.Domain.Entities;
using SmartCRM.Infrastructure.Persistence;
using MassTransit;
using SmartCRM.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace SmartCRM.Infrastructure.Plugins;

/// <summary>
/// A Semantic Kernel Plugin that provides CRM capabilities to the AI Agent.
/// </summary>
public sealed class CrmPlugin
{
    private readonly SmartCrmDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CrmPlugin> _logger;

    public CrmPlugin(SmartCrmDbContext dbContext, IPublishEndpoint publishEndpoint, ILogger<CrmPlugin> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [KernelFunction]
    [Description("Gets the total number of customers (leads) currently in the system.")]
    public async Task<int> GetCustomerCount()
    {
        Console.WriteLine("[CrmPlugin] Counting customers...");
        return await _dbContext.Customers.CountAsync();
    }

    [KernelFunction]
    [Description("Creates a new lead in the CRM system via async message bus.")]
    public async Task<string> CreateLead(
        [Description("The full name of the customer")] string name,
        [Description("The email address of the customer (optional, use 'no-email@example.com' if not provided)")] string email = "no-email@example.com")
    {
        Console.WriteLine($"[CrmPlugin] Creating lead: {name}, {email}");
        // Use IBus or IPublishEndpoint to send the command
        await _publishEndpoint.Publish(new CreateLeadCommand
        {
            Name = name,
            Email = email
        });

        return $"Lead creation request for {name} with email {email} has been successfully queued.";
    }

    [KernelFunction]
    [Description("Sends a marketing or welcome email to a customer.")]
    public async Task<string> SendEmailMarketing(
        [Description("The recipient's email address")] string email,
        [Description("The email template or purpose (e.g., Welcome, Promotion)")] string template)
    {
        _logger.LogInformation("Agent action: Sending {Template} email to {Email}", template, email);
        
        // Simulating email sending logic
        await Task.Delay(500); 

        return $"Email sent successfully to {email} using the '{template}' template.";
    }

    [KernelFunction]
    [Description("Checks the current inventory status of a product.")]
    public async Task<string> CheckInventory(
        [Description("The ID or name of the product")] string productId)
    {
        _logger.LogInformation("Agent action: Checking inventory for {ProductId}", productId);
        
        // Simulating inventory check
        return $"Product '{productId}' is currently in stock (15 units available).";
    }
}
