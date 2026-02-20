using AshWatch.Application.Common;
using AshWatch.Application.Contracts;
using AshWatch.Application.Dtos;
using AshWatch.Domain.Entities;
using AshWatch.Domain.Repositories;

namespace AshWatch.Application.Services;

public class LogService : ILogService
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

    private readonly ILogRepository _logRepository;

    public LogService(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<DefaultResponse<Log>> LogAsync(CreateLogRequest request)
    {
        var errors = ValidateCreateRequest(request);
        if (errors.Count > 0)
        {
            return DefaultResponse<Log>.Fail("Validation failed.", errors.ToArray());
        }

        var currentLogs = await _logRepository.GetAllAsync(request.TenantId, request.ProjectId, null, null);
        var nextId = currentLogs.Any() ? currentLogs.Max(x => x.Id) + 1 : 1;

        var log = MapToLog(request, nextId);
        await _logRepository.AddAsync(log);

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
        var nextIdByScope = new Dictionary<string, int>();

        foreach (var request in requests)
        {
            var scopeKey = $"{request.TenantId}:{request.ProjectId}";
            if (!nextIdByScope.TryGetValue(scopeKey, out var nextId))
            {
                var currentLogs = await _logRepository.GetAllAsync(request.TenantId, request.ProjectId, null, null);
                nextId = currentLogs.Any() ? currentLogs.Max(x => x.Id) + 1 : 1;
            }

            var log = MapToLog(request, nextId);
            logsToPersist.Add(log);
            nextIdByScope[scopeKey] = nextId + 1;
        }

        foreach (var log in logsToPersist)
        {
            await _logRepository.AddAsync(log);
        }

        return DefaultResponse<List<Log>>.Ok(logsToPersist, "Batch logs created successfully.");
    }

    public async Task<DefaultResponse<Log>> GetLogByIdAsync(int id, int tenantId, int projectId)
    {
        if (id <= 0 || tenantId <= 0 || projectId <= 0)
        {
            return DefaultResponse<Log>.Fail(
                "Validation failed.",
                "Id, tenantId and projectId must be greater than zero."
            );
        }

        var log = await _logRepository.GetByIdAsync(id, tenantId, projectId);
        if (log is null)
        {
            return DefaultResponse<Log>.Fail("Log not found.");
        }

        return DefaultResponse<Log>.Ok(log);
    }

    public async Task<DefaultResponse<List<Log>>> GetAllLogsAsync(GetLogsFilterRequest request)
    {
        var validationErrors = ValidateGetAllRequest(request);
        if (validationErrors.Count > 0)
        {
            return DefaultResponse<List<Log>>.Fail("Validation failed.", validationErrors.ToArray());
        }

        var logs = await _logRepository.GetAllAsync(
            request.TenantId,
            request.ProjectId,
            request.StartDate,
            request.EndDate
        );

        return DefaultResponse<List<Log>>.Ok(logs.ToList());
    }

    private static List<string> ValidateCreateRequest(CreateLogRequest request)
    {
        var errors = new List<string>();

        if (request.TenantId <= 0)
        {
            errors.Add("TenantId must be greater than zero.");
        }

        if (request.ProjectId <= 0)
        {
            errors.Add("ProjectId must be greater than zero.");
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

    private static List<string> ValidateGetAllRequest(GetLogsFilterRequest request)
    {
        var errors = new List<string>();

        if (request.TenantId <= 0)
        {
            errors.Add("TenantId must be greater than zero.");
        }

        if (request.ProjectId <= 0)
        {
            errors.Add("ProjectId must be greater than zero.");
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
        {
            errors.Add("StartDate cannot be greater than EndDate.");
        }

        return errors;
    }

    private static Log MapToLog(CreateLogRequest request, int id)
    {
        return new Log
        {
            Id = id,
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
