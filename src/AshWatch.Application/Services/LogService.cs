using AshWatch.Application.Contracts;
using AshWatch.Domain.Repositories;

namespace AshWatch.Application.Services;

public class LogService : ILogService
{
    private readonly List<string> _logs = [];
    private readonly ILogRepository _logRepository;

    public LogService(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public void Log(string message)
    {
        _logs.Add(message);
    }

    public void LogBatch(List<string> messages)
    {
        _logs.AddRange(messages);
    }

    public string GetLogById(int id)
    {
        return id >= 0 && id < _logs.Count ? _logs[id] : "";
    }

    public List<string> GetAllLogs()
    {
        return [.. _logs];
    }
}