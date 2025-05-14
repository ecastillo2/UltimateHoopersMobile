using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace UltimateHoopers.Helpers
{
    public static class DiagnosticHelper
    {
        private static StringBuilder _logBuilder = new StringBuilder();
        private static readonly string _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "UltimateHoopers_Diagnostic.log");

        public static void Log(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logMessage = $"[{timestamp}] {message}";

                _logBuilder.AppendLine(logMessage);

                // Write to file as we go
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Ignore any errors in logging to avoid circular problems
            }
        }

        public static void LogException(Exception ex, string context = "")
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logMessage = $"[{timestamp}] EXCEPTION in {context}: {ex.GetType().Name}: {ex.Message}";
                var stackTrace = $"StackTrace: {ex.StackTrace}";

                _logBuilder.AppendLine(logMessage);
                _logBuilder.AppendLine(stackTrace);

                if (ex.InnerException != null)
                {
                    _logBuilder.AppendLine($"Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    _logBuilder.AppendLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
                }

                _logBuilder.AppendLine();

                // Write to file as we go
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                File.AppendAllText(_logFilePath, stackTrace + Environment.NewLine);

                if (ex.InnerException != null)
                {
                    File.AppendAllText(_logFilePath, $"Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}" + Environment.NewLine);
                    File.AppendAllText(_logFilePath, $"Inner StackTrace: {ex.InnerException.StackTrace}" + Environment.NewLine);
                }

                File.AppendAllText(_logFilePath, Environment.NewLine);
            }
            catch
            {
                // Ignore any errors in logging to avoid circular problems
            }
        }
    }
}