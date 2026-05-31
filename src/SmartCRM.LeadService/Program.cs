using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using SmartCRM.Infrastructure;
using SmartCRM.LeadService.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// Register Infrastructure and MassTransit with Consumers in one go
builder.Services.AddInfrastructure(builder.Configuration, x =>
{
    x.AddConsumer<CreateLeadConsumer>();
});

var host = builder.Build();
host.Run();
