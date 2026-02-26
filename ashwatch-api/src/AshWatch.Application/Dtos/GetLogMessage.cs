namespace AshWatch.Application.Dtos;

public record GetLogMessage(
    string Id,
    string Level,
    string Message,
    string Author,
    Guid TenantId,
    Guid ProjectId,
    Guid AuthorId
);