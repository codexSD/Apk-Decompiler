namespace Apk_Decompiler.Models
{
    public class ToolInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string LocalPath { get; set; } = string.Empty;
        public bool IsInstalled { get; set; }
        public bool IsLatestVersion { get; set; }
    }

    public class GitHubRelease
    {
        public string tag_name { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public bool prerelease { get; set; }
        public GitHubAsset[] assets { get; set; } = Array.Empty<GitHubAsset>();
    }

    public class GitHubAsset
    {
        public string name { get; set; } = string.Empty;
        public string browser_download_url { get; set; } = string.Empty;
        public long size { get; set; }
    }
}
