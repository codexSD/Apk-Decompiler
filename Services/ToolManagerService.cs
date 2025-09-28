using System.IO;
using System.IO.Compression;
using System.Net.Http;
using Newtonsoft.Json;
using Apk_Decompiler.Models;

namespace Apk_Decompiler.Services
{
    public class ToolManagerService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string toolsDirectory;

        public ToolManagerService()
        {
            toolsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");
            EnsureToolsDirectory();
            
            // Configure HttpClient with User-Agent to avoid GitHub API rate limiting
            if (!httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "APK-Decompiler/1.0");
            }
        }

        private void EnsureToolsDirectory()
        {
            if (!Directory.Exists(toolsDirectory))
            {
                Directory.CreateDirectory(toolsDirectory);
                LoggingService.LogInfo($"Created tools directory: {toolsDirectory}");
            }
        }

        public async Task<ToolInfo> CheckApkToolAsync()
        {
            var toolInfo = new ToolInfo
            {
                Name = "APKTool",
                LocalPath = Path.Combine(toolsDirectory, "apktool.jar")
            };

            try
            {
                // Check if APKTool exists locally
                toolInfo.IsInstalled = File.Exists(toolInfo.LocalPath);

                // Try to get latest version from GitHub API
                try
                {
                    var response = await httpClient.GetStringAsync("https://api.github.com/repos/iBotPeaches/Apktool/releases/latest");
                    var release = JsonConvert.DeserializeObject<GitHubRelease>(response);

                    if (release != null)
                    {
                        toolInfo.Version = release.tag_name.TrimStart('v');
                        
                        // Find the JAR file in assets
                        var jarAsset = release.assets.FirstOrDefault(a => a.name.EndsWith(".jar") && !a.name.Contains("sources"));
                        if (jarAsset != null)
                        {
                            toolInfo.DownloadUrl = jarAsset.browser_download_url;
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    LoggingService.LogWarning($"GitHub API failed for APKTool, using fallback: {ex.Message}");
                    
                    // Fallback to known stable version
                    toolInfo.Version = "2.9.3";
                    toolInfo.DownloadUrl = "https://github.com/iBotPeaches/Apktool/releases/download/v2.9.3/apktool_2.9.3.jar";
                }

                toolInfo.IsLatestVersion = toolInfo.IsInstalled; // For now, assume installed version is latest
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to check APKTool: {ex.Message}");
                
                // Set fallback values even on complete failure
                toolInfo.Version = "2.9.3";
                toolInfo.DownloadUrl = "https://github.com/iBotPeaches/Apktool/releases/download/v2.9.3/apktool_2.9.3.jar";
            }

            return toolInfo;
        }

        public async Task<ToolInfo> CheckUberSignerAsync()
        {
            var toolInfo = new ToolInfo
            {
                Name = "Uber APK Signer",
                LocalPath = Path.Combine(toolsDirectory, "uber-apk-signer.jar")
            };

            try
            {
                // Check if UberSigner exists locally
                toolInfo.IsInstalled = File.Exists(toolInfo.LocalPath);

                // Try to get latest version from GitHub API
                try
                {
                    var response = await httpClient.GetStringAsync("https://api.github.com/repos/patrickfav/uber-apk-signer/releases/latest");
                    var release = JsonConvert.DeserializeObject<GitHubRelease>(response);

                    if (release != null)
                    {
                        toolInfo.Version = release.tag_name.TrimStart('v');
                        
                        // Find the JAR file in assets
                        var jarAsset = release.assets.FirstOrDefault(a => a.name.EndsWith(".jar"));
                        if (jarAsset != null)
                        {
                            toolInfo.DownloadUrl = jarAsset.browser_download_url;
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    LoggingService.LogWarning($"GitHub API failed for Uber APK Signer, using fallback: {ex.Message}");
                    
                    // Fallback to known stable version
                    toolInfo.Version = "1.3.0";
                    toolInfo.DownloadUrl = "https://github.com/patrickfav/uber-apk-signer/releases/download/v1.3.0/uber-apk-signer-1.3.0.jar";
                }

                toolInfo.IsLatestVersion = toolInfo.IsInstalled; // For now, assume installed version is latest
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to check Uber APK Signer: {ex.Message}");
                
                // Set fallback values even on complete failure
                toolInfo.Version = "1.3.0";
                toolInfo.DownloadUrl = "https://github.com/patrickfav/uber-apk-signer/releases/download/v1.3.0/uber-apk-signer-1.3.0.jar";
            }

            return toolInfo;
        }

        public async Task<bool> DownloadToolAsync(ToolInfo toolInfo, IProgress<int>? progress = null)
        {
            try
            {
                LoggingService.LogInfo($"Downloading {toolInfo.Name} from {toolInfo.DownloadUrl}");

                using var response = await httpClient.GetAsync(toolInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var downloadedBytes = 0L;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(toolInfo.LocalPath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    if (totalBytes > 0)
                    {
                        var progressPercentage = (int)((downloadedBytes * 100) / totalBytes);
                        progress?.Report(progressPercentage);
                    }
                }

                LoggingService.LogInfo($"Successfully downloaded {toolInfo.Name} to {toolInfo.LocalPath}");
                return true;
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to download {toolInfo.Name}: {ex.Message}");
                return false;
            }
        }

        public string GetApkToolPath()
        {
            return Path.Combine(toolsDirectory, "apktool.jar");
        }

        public string GetUberSignerPath()
        {
            return Path.Combine(toolsDirectory, "uber-apk-signer.jar");
        }

        public string GetWorkspaceDirectory()
        {
            var workspaceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workspace");
            if (!Directory.Exists(workspaceDir))
            {
                Directory.CreateDirectory(workspaceDir);
            }
            return workspaceDir;
        }

        public string GetOutputDirectory()
        {
            var outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            return outputDir;
        }
    }
}
