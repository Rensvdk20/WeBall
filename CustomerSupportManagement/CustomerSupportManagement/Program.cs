using System.Reflection;
using Microsoft.EntityFrameworkCore;
using CustomerSupportManagement.Domain.Entities;
using CustomerSupportManagement.DomainServices.Consumers;
using CustomerSupportManagement.DomainServices.Interfaces;
using CustomerSupportManagement.DomainServices.Services;
using CustomerSupportManagement.Infrastructure.Middleware;
using CustomerSupportManagement.Infrastructure.SQLRepo;
using MassTransit;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ISupportAgentService, SupportAgentService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();

builder.Services.AddScoped<ISupportAgentRepo, SQLSupportAgentRepo>();
builder.Services.AddScoped<ISupportTicketRepo, SQLSupportTicketRepo>();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddMassTransit(x =>
{
    x.SetEndpointNameFormatter(
        new DefaultEndpointNameFormatter(prefix: Assembly.GetExecutingAssembly().GetName().Name));
    
    // add consumers using this following line
    x.AddConsumer<CustomerUpdatedConsumer>();
    x.AddConsumer<CustomerDeletedConsumer>();
		
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["WeBall:RabbitMqHost"], "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});


var configuration = builder.Configuration;
var connectionString = configuration["WeBall:MySQLDBConn"];
builder.Services.AddDbContext<SQLDbContext>(opts =>
{
    Policy
        .Handle<Exception>()
        .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
        .Execute(() =>
        {
            opts.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                dbOpts => { dbOpts.EnableRetryOnFailure(100, TimeSpan.FromSeconds(10), null); });
        });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

#region DbMigration 

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SQLDbContext>();
    dbContext.Migrate();
}

#endregion

app.UseMiddleware<ErrorHandlingMiddleware>();

// # SupportAgent #
app.MapGet("/supportagents", async (ISupportAgentService supportAgentService) => await supportAgentService.GetAll());

app.MapGet("/supportagents/{id}", async (ISupportAgentService supportAgentService, string id) => await supportAgentService.GetById(id));

app.MapPost("/supportagents", async (ISupportAgentService supportAgentService, SupportAgent supportAgent) =>
{
    await supportAgentService.Create(supportAgent);
    return Results.Created($"/supportagents/{supportAgent.Id}", new { code = 201, message = "SupportAgent created successfully" });
});

app.MapPut("/supportagents/{id}", async (ISupportAgentService supportAgentService, string id, SupportAgent supportAgent) =>
{
    await supportAgentService.Update(id, supportAgent);
    return Results.Ok(new { code = 200, message = "SupportAgent updated successfully" });
});

app.MapDelete("/supportagents/{id}", async (ISupportAgentService supportAgentService, string id) =>
{
    await supportAgentService.Delete(id);
    return Results.Ok(new { code = 200, message = "SupportAgent deleted successfully" });
});

// # SupportTicket #
app.MapGet("/supporttickets", async (ISupportTicketService supportTicketService) => await supportTicketService.GetAll());

app.MapGet("/supporttickets/{id}", async (ISupportTicketService supportTicketService, string id) => await supportTicketService.GetById(id));

app.MapPost("/supporttickets", async (ISupportTicketService supportTicketService, SupportTicket supportTicket) =>
{
    await supportTicketService.Create(supportTicket);
    return Results.Created($"/supporttickets/{supportTicket.Id}", new { code = 201, message = "SupportTicket created successfully" });
});

app.MapPut("/supporttickets/{id}", async (ISupportTicketService supportTicketService, string id, SupportTicket supportTicket) =>
{
    await supportTicketService.Update(id, supportTicket);
    return Results.Ok(new { code = 200, message = "SupportTicket updated successfully" });
});

app.MapDelete("/supporttickets/{id}", async (ISupportTicketService supportTicketService, string id) =>
{
    await supportTicketService.Delete(id);
    return Results.Ok(new { code = 200, message = "SupportTicket deleted successfully" });
});

app.MapPut("/supporttickets/{ticketId}/assign/{agentId}", async (ISupportTicketService supportTicketService, string ticketId, string agentId) =>
{
    await supportTicketService.AssignSupportAgent(ticketId, agentId);
    return Results.Ok(new { code = 200, message = "SupportAgent assigned successfully" });
});

app.MapPut("/supporttickets/{ticketId}/close", async (ISupportTicketService supportTicketService, string ticketId) =>
{
    await supportTicketService.Close(ticketId);
    return Results.Ok(new { code = 200, message = "SupportTicket closed successfully" });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();