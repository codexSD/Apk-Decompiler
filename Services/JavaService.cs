using System.Diagnostics;
using System.IO;

namespace Apk_Decompiler.Services
{
    public class JavaService
    {
        private const string JAVA_DOWNLOAD_URL = "https://www.oracle.com/java/technologies/downloads/";

        public static bool IsJavaInstalled()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "java",
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return process.ExitCode == 0 && output.Contains("version");
            }
            catch
            {
                return false;
            }
        }

        public static string GetJavaVersion()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "java",
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    var lines = output.Split('\n');
                    if (lines.Length > 0)
                    {
                        var versionLine = lines[0];
                        var start = versionLine.IndexOf('"') + 1;
                        var end = versionLine.LastIndexOf('"');
                        if (start > 0 && end > start)
                        {
                            return versionLine.Substring(start, end - start);
                        }
                    }
                }
            }
            catch
            {
                // Ignore
            }

            return "Unknown";
        }

        public static void OpenJavaDownloadPage()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = JAVA_DOWNLOAD_URL,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to open Java download page: {ex.Message}");
            }
        }
    }
}
