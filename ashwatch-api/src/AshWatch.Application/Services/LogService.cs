using AshWatch.Application.Common;
using AshWatch.Application.Contracts;
using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;

namespace AshWatch.Application.Services;

public class LogService(IPublisher publisher) : ILogService
{
    private static readonly HashSet<string> AllowedLevels =
    [
        "TRACE",
        "DEBUG",
        "INFO",
        "WARN",
        "ERROR",
        "FATAL"
    ];

    public async Task<DefaultResponse<Log>> LogAsync(CreateLogRequest request)
    {
        var errors = ValidateCreateRequest(request);
        if (errors.Count > 0)
        {
            return DefaultResponse<Log>.Fail("Validation failed.", errors.ToArray());
        }

        Log log = CreateLogRequest.MapToLog(request);

        await publisher.PublishAsync(log);

        return DefaultResponse<Log>.Ok(log, "Log created successfully.");
    }

    public async Task<DefaultResponse<List<Log>>> LogBatchAsync(List<CreateLogRequest> requests)
    {
        if (requests.Count == 0)
        {
            return DefaultResponse<List<Log>>.Fail("Validation failed.", "Batch request cannot be empty.");
        }

        var validationErrors = requests
            .SelectMany((request, index) => ValidateCreateRequest(request).Select(error => $"Item {index}: {error}"))
            .ToList();

        if (validationErrors.Count > 0)
        {
            return DefaultResponse<List<Log>>.Fail("Validation failed.", validationErrors.ToArray());
        }

        var logsToPersist = new List<Log>(requests.Count);

        foreach (var request in requests)
        {
            var log = CreateLogRequest.MapToLog(request);
            logsToPersist.Add(log);
        }

        foreach (var log in logsToPersist)
        {
            await publisher.PublishAsync(log);
        }

        return DefaultResponse<List<Log>>.Ok(logsToPersist, "Batch logs created successfully.");
    }

    private static List<string> ValidateCreateRequest(CreateLogRequest request)
    {
        var errors = new List<string>();

        if (request.TenantId == Guid.Empty)
        {
            errors.Add("TenantId must be a valid GUID.");
        }

        if (request.ProjectId == Guid.Empty)
        {
            errors.Add("ProjectId must be a valid GUID.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            errors.Add("Message is required.");
        }

        if (request.Message.Length > 4000)
        {
            errors.Add("Message cannot exceed 4000 characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.Level))
        {
            var normalizedLevel = request.Level.Trim().ToUpperInvariant();
            if (!AllowedLevels.Contains(normalizedLevel))
            {
                errors.Add($"Level '{request.Level}' is invalid. Allowed values: {string.Join(", ", AllowedLevels)}.");
            }
        }

        return errors;
    }
}
