using AshWatch.Domain.Entities;

namespace AshWatch.Application.Dtos;

public class CreateLogRequest
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public static Log MapToLog(CreateLogRequest request)
    {
        return new Log
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ProjectId = request.ProjectId,
            Author = string.IsNullOrWhiteSpace(request.Author) ? "system" : request.Author.Trim(),
            Message = request.Message.Trim(),
            Level = NormalizeLevel(request.Level),
            Timestamp = request.Timestamp == default ? DateTime.UtcNow : request.Timestamp
        };
    }

    private static string NormalizeLevel(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return "INFO";
        }

        return level.Trim().ToUpperInvariant();
    }
}
