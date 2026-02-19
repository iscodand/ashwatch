namespace AshWatch.Application.Contracts;

public interface ILogService
{
    void Log(string message);
    void LogBatch(List<string> messages);
    string GetLogById(int id);
    List<string> GetAllLogs();
}