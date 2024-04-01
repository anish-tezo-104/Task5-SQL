namespace EMS.Common.Logging;

public interface ILogger
{
    void LogError(string message, bool newLine = true);
    void LogSuccess(string message, bool newLine = true);
    void LogWarning(string message, bool newLine = true);
}