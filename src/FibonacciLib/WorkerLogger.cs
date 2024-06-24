using Serilog;
using System.Text;

namespace FibonacciLib;

public static class WorkerLogger
{
    private static ILogger _consoleLogger;

    static WorkerLogger()
    {
        _consoleLogger = new LoggerConfiguration()
           .WriteTo.Console()
           .CreateLogger();
    }

    public static void LogMessage(string message)
    {
        var logString = $"{message}";

        _consoleLogger.Information(logString);

    }

    public static void LogException(this Exception ex, string extraMessage = "")
    {
        var logString = new StringBuilder();

        logString.Append("An unexpected system error occurred: ");

        logString.AppendLine(ex?.Message);

        if (!string.IsNullOrEmpty(extraMessage))
        {
            logString.AppendLine($"Extra message: {extraMessage}");
        }

        logString.AppendLine($"Stacktrace: {ex?.StackTrace}");

        _consoleLogger.Error(logString.ToString());

    }
}