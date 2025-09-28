using System.Diagnostics;
using System.IO;
using Apk_Decompiler.Models;

namespace Apk_Decompiler.Services
{
    public class ApkProcessorService
    {
        private readonly ToolManagerService toolManager;
        private readonly LocalizationService localization;

        public ApkProcessorService(ToolManagerService toolManager)
        {
            this.toolManager = toolManager;
            this.localization = LocalizationService.Instance;
        }

        public async Task<bool> DecompileApkAsync(string apkPath, IProgress<string>? progressCallback = null)
        {
            try
            {
                var workspaceDir = toolManager.GetWorkspaceDirectory();
                var apkToolPath = toolManager.GetApkToolPath();

                // Clear existing workspace
                if (Directory.Exists(workspaceDir))
                {
                    Directory.Delete(workspaceDir, true);
                    Directory.CreateDirectory(workspaceDir);
                }

                var apkName = Path.GetFileNameWithoutExtension(apkPath);
                var outputDir = Path.Combine(workspaceDir, apkName);

                progressCallback?.Report(localization.StartingDecompilation);
                LoggingService.LogInfo($"Decompiling APK: {apkPath} to {outputDir}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "java",
                        Arguments = $"-jar \"{apkToolPath}\" d \"{apkPath}\" -o \"{outputDir}\" -f",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = workspaceDir
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        progressCallback?.Report(e.Data);
                        LoggingService.LogInfo($"APKTool Output: {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        progressCallback?.Report($"Error: {e.Data}");
                        LoggingService.LogError($"APKTool Error: {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    progressCallback?.Report(localization.DecompilationCompletedSuccessfully);
                    LoggingService.LogInfo($"Successfully decompiled APK to {outputDir}");
                    return true;
                }
                else
                {
                    progressCallback?.Report(localization.FailedWithExitCode(process.ExitCode));
                    LoggingService.LogError($"APKTool failed with exit code: {process.ExitCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                progressCallback?.Report(localization.ErrorDuring("decompilation", ex.Message));
                LoggingService.LogError($"Decompilation error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RecompileApkAsync(string projectName, IProgress<string>? progressCallback = null)
        {
            try
            {
                var workspaceDir = toolManager.GetWorkspaceDirectory();
                var apkToolPath = toolManager.GetApkToolPath();
                var projectDir = Path.Combine(workspaceDir, projectName);
                var outputDir = toolManager.GetOutputDirectory();

                if (!Directory.Exists(projectDir))
                {
                    progressCallback?.Report(localization.ProjectDirectoryNotFound);
                    return false;
                }

                var outputApkPath = Path.Combine(outputDir, $"{projectName}_recompiled.apk");

                progressCallback?.Report(localization.StartingRecompilation);
                LoggingService.LogInfo($"Recompiling project: {projectDir} to {outputApkPath}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "java",
                        Arguments = $"-jar \"{apkToolPath}\" b \"{projectDir}\" -o \"{outputApkPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = workspaceDir
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        progressCallback?.Report(e.Data);
                        LoggingService.LogInfo($"APKTool Build Output: {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        progressCallback?.Report($"Error: {e.Data}");
                        LoggingService.LogError($"APKTool Build Error: {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0 && File.Exists(outputApkPath))
                {
                    progressCallback?.Report(localization.RecompilationCompletedSuccessfully);
                    LoggingService.LogInfo($"Successfully recompiled APK to {outputApkPath}");
                    return true;
                }
                else
                {
                    progressCallback?.Report(localization.FailedWithExitCode(process.ExitCode));
                    LoggingService.LogError($"APKTool build failed with exit code: {process.ExitCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                progressCallback?.Report(localization.ErrorDuring("recompilation", ex.Message));
                LoggingService.LogError($"Recompilation error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SignApkAsync(string apkPath, IProgress<string>? progressCallback = null)
        {
            try
            {
                var uberSignerPath = toolManager.GetUberSignerPath();
                var outputDir = Path.GetDirectoryName(apkPath);
                var apkName = Path.GetFileNameWithoutExtension(apkPath);
                var signedApkPath = Path.Combine(outputDir!, $"{apkName}_signed.apk");

                progressCallback?.Report(localization.StartingApkSigning);
                LoggingService.LogInfo($"Signing APK: {apkPath}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "java",
                        Arguments = $"-jar \"{uberSignerPath}\" --apks \"{apkPath}\" --out \"{outputDir}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = outputDir
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        progressCallback?.Report(e.Data);
                        LoggingService.LogInfo($"UberSigner Output: {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        progressCallback?.Report(localization.Signing(e.Data));
                        LoggingService.LogInfo($"UberSigner Info: {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    progressCallback?.Report(localization.ApkSignedSuccessfully);
                    LoggingService.LogInfo($"Successfully signed APK");
                    return true;
                }
                else
                {
                    progressCallback?.Report(localization.FailedWithExitCode(process.ExitCode));
                    LoggingService.LogError($"UberSigner failed with exit code: {process.ExitCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                progressCallback?.Report(localization.ErrorDuring("signing", ex.Message));
                LoggingService.LogError($"Signing error: {ex.Message}");
                return false;
            }
        }

        public void OpenWorkspaceFolder()
        {
            try
            {
                var workspaceDir = toolManager.GetWorkspaceDirectory();
                Process.Start(new ProcessStartInfo
                {
                    FileName = workspaceDir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to open workspace folder: {ex.Message}");
            }
        }

        public void OpenOutputFolder()
        {
            try
            {
                var outputDir = toolManager.GetOutputDirectory();
                Process.Start(new ProcessStartInfo
                {
                    FileName = outputDir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to open output folder: {ex.Message}");
            }
        }

        public string[] GetDecompiledProjects()
        {
            try
            {
                var workspaceDir = toolManager.GetWorkspaceDirectory();
                if (Directory.Exists(workspaceDir))
                {
                    return Directory.GetDirectories(workspaceDir)
                        .Select(Path.GetFileName)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToArray()!;
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get decompiled projects: {ex.Message}");
            }

            return Array.Empty<string>();
        }
    }
}
