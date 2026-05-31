using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using SmartCRM.Domain.Entities;
using SmartCRM.Infrastructure.Persistence;
using SmartCRM.Shared.Contracts;

namespace SmartCRM.LeadService.Consumers;

public sealed class CreateLeadConsumer : IConsumer<CreateLeadCommand>
{
    private readonly SmartCrmDbContext _dbContext;
    private readonly ILogger<CreateLeadConsumer> _logger;

    public CreateLeadConsumer(SmartCrmDbContext dbContext, ILogger<CreateLeadConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateLeadCommand> context)
    {
        var command = context.Message;
        _logger.LogInformation("Processing CreateLeadCommand: {Name} ({Email})", command.Name, command.Email);

        var customer = new Customer
        {
            FullName = command.Name,
            Email = command.Email,
            Status = "Lead"
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Lead created successfully for {Name} with ID: {Id}", command.Name, customer.Id);
    }
}
