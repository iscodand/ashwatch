using AshWatch.Application.Contracts;

namespace AshWatch.Application.Services;

public class LogService : ILogService
{
    private readonly List<string> _logs = [];

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