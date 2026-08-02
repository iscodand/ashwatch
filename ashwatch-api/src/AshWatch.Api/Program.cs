using Amazon.SimpleNotificationService;
using AshWatch.Application.Contracts;
using AshWatch.Application.Services;
using AshWatch.Domain.Repositories;
using AshWatch.Infrastructure.AWS;
using AshWatch.Infrastructure.Data;
using AshWatch.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var awsServiceUrl = builder.Configuration["AWS:ServiceURL"];
if (!string.IsNullOrEmpty(awsServiceUrl))
{
    builder.Services.AddSingleton<IAmazonSimpleNotificationService>(
        new AmazonSimpleNotificationServiceClient(
            new AmazonSimpleNotificationServiceConfig { ServiceURL = awsServiceUrl }
        )
    );
}
else
{
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
}

builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddSingleton<IPublisher, Publisher>();

var dbHost = builder.Configuration["DB_HOST"];
var connectionString = string.IsNullOrEmpty(dbHost)
    ? builder.Configuration.GetConnectionString("PostgresDb")
    : new NpgsqlConnectionStringBuilder
    {
        Host = dbHost,
        Port = int.Parse(builder.Configuration["DB_PORT"] ?? "5432"),
        Database = builder.Configuration["DB_NAME"],
        Username = builder.Configuration["DB_USERNAME"],
        Password = builder.Configuration["DB_PASSWORD"],
    }.ConnectionString;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "AshWatch API";
    config.Title = "AshWatch API v1";
    config.Version = "v1";
});

var app = builder.Build();

app.UsePathBase("/commands");

app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = "AshWatch API Documentation";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/health", () => Results.Ok("ok"));

app.MapControllers();

app.Run();
