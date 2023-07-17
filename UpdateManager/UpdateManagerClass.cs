using System.Diagnostics;
using System.Reflection;

namespace UpdateManager;

public class UpdateManagerClass
{
    private static readonly HttpClient Client = new();
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


    private static async Task<string> GetLatestVersionFromUrlAsync(string url)
    {
        try
        {
            var latestVersion = await Client.GetStringAsync(url);
            return latestVersion;
        }
        catch (Exception ex)
        {
            // Handle exceptions as necessary
            Debug.WriteLine($@"An error occurred: {ex.Message}");
        }

        return "1.0.0";
    }

    private static async Task<Version> GetLatestVersionFromExeUrlAsync(string url, string localFilePath)
    {
        try
        {
            var response = await Client.GetAsync(url);

            using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fileStream);
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(localFilePath);
            if (versionInfo.ProductVersion != null) return new Version(versionInfo.ProductVersion);
        }
        catch (Exception ex)
        {
            // Handle exceptions as necessary, for example log the error message
            Debug.WriteLine($@"An error occurred: {ex.Message}");
        }

        return new Version("1.0.0");
    }

    private static Version? GetCurrentVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version;
    }


    private static async Task DownloadLatestVersionAsync(string url)
    {
        try
        {
            var response = await Client.GetAsync(url);
            using var fileStream = new FileStream("update.exe", FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            // Handle exceptions as necessary, for example log the error message
            Debug.WriteLine($@"An error occurred: {ex.Message}");
        }

        // You will then need to implement how you would like to apply the update.
        // This could involve shutting down the current application and running
        // the update installer, replacing the current application binary, etc.
    }
}