using System.Reflection;
using InventoryManagement.Domain.Entities;
using InventoryManagement.DomainServices.Consumers;
using InventoryManagement.DomainServices.Interfaces;
using InventoryManagement.DomainServices.Services;
using InventoryManagement.Infrastructure.Middleware;
using InventoryManagement.Infrastructure.MongoRepo;
using InventoryManagement.Infrastructure.SqlRepo;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(s =>
{
    var configuration = builder.Configuration;
    var connectionString = configuration["WeBall:MongoDBConn"];
    return new MongoClient(connectionString);
});

var configuration = builder.Configuration;
var connectionString = configuration["WeBall:MySQLDBConn"];

builder.Services.AddDbContext<SqlDbContext>(opts =>
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


builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("InventoryManagement");
});

builder.Services.AddScoped<IMongoCollection<Product>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<Product>("Products");
});

// Adding the MongoDbContext
builder.Services.AddSingleton<MongoDbContext>();

// Add services to the container.
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductMongoRepository, ProductMongoRepository>();
builder.Services.AddScoped<IProductCommandRepository, ProductSqlRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();
    x.AddConsumer<ProductUpdatedConsumer>();
    x.AddConsumer<ProductDeletedConsumer>();

    x.SetEndpointNameFormatter(
        new DefaultEndpointNameFormatter(prefix: Assembly.GetExecutingAssembly().GetName().Name));

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

var app = builder.Build();

#region DbMigration

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SqlDbContext>();
    dbContext.Migrate();
}

#endregion


app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/products", (IProductService productService) => productService.GetProducts());
app.MapGet("/product/{id:guid}", (IProductService productService, Guid id) => productService.GetProductById(id));
app.MapPost("/product", (IProductService productService, Product product) => productService.CreateProduct(product));
app.MapPut("/product/{id:guid}",
    (IProductService productService, Guid id, Product product) => productService.UpdateProduct(id, product));
app.MapDelete("/product/{id:guid}", (IProductService productService, Guid id) => productService.DeleteProduct(id));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();