using AshWatch.Application.Contracts;
using AshWatch.Application.Services;
using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.Data;
using AshWatch.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb"))
);

builder.Services.AddControllers();
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

app.MapControllers();

app.Run();
