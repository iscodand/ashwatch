using AshWatch.Application.Contracts;
using AshWatch.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ILogService, LogService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});

var app = builder.Build();

app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = "AshWatch API Documentation";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});

app.MapPost("/logs", (string message) => $"Echo: {message}");

app.MapPost("logs/batch", (List<string> messages) => $"Received {messages.Count} messages.");

app.MapGet("/logs/{id}", (int id) => $"Log ID: {id}");

app.MapGet("logs/", () => "Fetching all logs...");

app.Run();