using AshWatch.Application.Contracts;
using AshWatch.Application.Dtos;
using AshWatch.Application.Services;
using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
using AshWatch.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<ILogRepository, LogRepository>();

builder.Services.AddSingleton<DataContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "AshWatch API";
    config.Title = "AshWatch API v1";
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

app.MapPost("/logs", (CreateLogRequest request, ILogService logService) =>
{
    return logService.LogAsync(request);
});

app.MapPost("/logs/batch", (List<CreateLogRequest> requests, ILogService logService) =>
{
    return logService.LogBatchAsync(requests);
});

app.MapGet(
    "/logs/{id}",
    (int id, int tenantId, int projectId, ILogService logService) => logService.GetLogByIdAsync(id, tenantId, projectId)
);

app.MapGet("/logs/", (int tenantId, int projectId, DateTime? startDate, DateTime? endDate, ILogService logService) =>
{
    var request = new GetLogsFilterRequest
    {
        TenantId = tenantId,
        ProjectId = projectId,
        StartDate = startDate,
        EndDate = endDate
    };

    return logService.GetAllLogsAsync(request);
});

app.Run();
