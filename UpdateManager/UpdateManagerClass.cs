using System.Diagnostics;
using System.Net;
using System.Reflection;

public class UpdateManagerClass
{
    private readonly string _downloadUrl;
    private readonly string _versionCheckUrl;

    public UpdateManagerClass(string versionCheckUrl, string downloadUrl)
    {
        _versionCheckUrl = versionCheckUrl;
        _downloadUrl = downloadUrl;
    }

    public async Task CheckForUpdatesAsync()
    {
        var latestVersion =
            await GetLatestVersionFromExeUrlAsync("https://example.com/latest-version.exe", "latest-version.exe");

        var currentVersion = GetCurrentVersion();

        if (currentVersion < latestVersion) await DownloadLatestVersionAsync(_downloadUrl);
    }

    private async Task<string> GetLatestVersionFromUrlAsync(string url)
    {
        using (var webClient = new WebClient())
        {
            var versionString = await webClient.DownloadStringTaskAsync(url);
            return versionString.Trim();
        }
    }

    public async Task<Version> GetLatestVersionFromExeUrlAsync(string url, string localFilePath)
    {
        using (var webClient = new WebClient())
        {
            await webClient.DownloadFileTaskAsync(url, localFilePath);
        }

        var versionInfo = FileVersionInfo.GetVersionInfo(localFilePath);
        if (versionInfo.ProductVersion != null) return new Version(versionInfo.ProductVersion);
        return new Version("1.0.0");
    }

    private Version GetCurrentVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version;
    }

    private async Task DownloadLatestVersionAsync(string url)
    {
        using (var webClient = new WebClient())
        {
            await webClient.DownloadFileTaskAsync(url, "update.exe");
        }

        // You will then need to implement how you would like to apply the update.
        // This could involve shutting down the current application and running
        // the update installer, replacing the current application binary, etc.
    }
}