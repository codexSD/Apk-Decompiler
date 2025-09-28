using System.IO;

namespace Apk_Decompiler.Services
{
    public static class LoggingService
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");
        private static readonly object LogLock = new object();

        static LoggingService()
        {
            // Ensure logs directory exists
            var logDir = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public static void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        public static void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        public static void LogDebug(string message)
        {
            WriteLog("DEBUG", message);
        }

        private static void WriteLog(string level, string message)
        {
            lock (LogLock)
            {
                try
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                }
                catch
                {
                    // Ignore logging errors to prevent infinite loops
                }
            }
        }

        public static string GetLogFilePath()
        {
            return LogFilePath;
        }

        public static void ClearLogs()
        {
            lock (LogLock)
            {
                try
                {
                    if (File.Exists(LogFilePath))
                    {
                        File.Delete(LogFilePath);
                    }
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}
