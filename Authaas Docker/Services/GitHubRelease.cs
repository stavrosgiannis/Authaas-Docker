using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace Authaas_Docker.Services;

public class GitHubRelease
{
    private static readonly HttpClient Client = new();
    public string? tag_name { get; set; }

    /// <summary>
    ///     Calculates the current application hash.
    /// </summary>
    /// <returns>The current application hash.</returns>
    public static string CalculateCurrentAppHash()
    {
        var location = Assembly.GetEntryAssembly()?.Location;
        if (location != null)
            using (var stream = File.OpenRead(location))
            {
                using var sha = SHA1.Create();
                var hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

        return "";
    }

    /// <summary>
    ///     Calculates the SHA1 hash of a file.
    /// </summary>
    /// <param name="filePath">The path of the file to calculate the hash for.</param>
    /// <returns>The SHA1 hash of the file.</returns>
    public static string CalculateFileHash(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            using var sha = SHA1.Create();
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public static async Task DownloadLatestRelease(string repoOwner, string repoName, string tag,
        ProgressBar progressBar)
    {
        var url = $"https://github.com/{repoOwner}/{repoName}/releases/download/{tag}/authaas-docker.zip";

        using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength ?? 0;

        using var stream = await response.Content.ReadAsStreamAsync();
        using var fileStream = File.Create("AuthaasDocker.zip");

        var totalBytesCopied = 0L;
        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead);
            totalBytesCopied += bytesRead;

            var progress = (int)(100L * totalBytesCopied / contentLength);
            progressBar.Invoke(new Action(() => progressBar.Value = progress));
        }
    }


    public static async Task<GenericResult> DownloadLatestReleaseIfUpdateAvailable(Version? currentVersion,
        string repoOwner,
        string repoName, ProgressBar progressBar)
    {
        var latestTag = await GetLatestReleaseTagAsync(repoOwner, repoName);
        if (latestTag == null) throw new Exception("Could not retrieve latest release tag.");

        // Assuming that your tags are in the format "vX.Y.Z.X", we remove the leading 'v' 
        // to convert the tag into a version string
        var latestVersion = new Version(latestTag);

        // Compare the current version with the latest version
        if (currentVersion != null && currentVersion.CompareTo(latestVersion) < 0)
        {
            await DownloadLatestRelease(repoOwner, repoName, latestTag, progressBar);
            return GenericResult.Ok("Downloading latest release..");
        }

        return GenericResult.Ok("Newest Version already!");
    }


    /// <summary>
    ///     Gets the latest release tag from a GitHub repository.
    /// </summary>
    /// <param name="repoOwner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <returns>The latest release tag, or null if the repository does not exist.</returns>
    public static async Task<string?> GetLatestReleaseTagAsync(string repoOwner, string repoName)
    {
        var url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";
        Client.DefaultRequestHeaders.UserAgent
            .ParseAdd("Authaas-Docker-Updater"); // GitHub API requires a User-Agent header
        var json = await Client.GetStringAsync(url);
        var release = JsonSerializer.Deserialize<GitHubRelease>(json);

        // Remove the "commit-" prefix from the tag
        var tag = release?.tag_name;
        if (tag != null && tag.StartsWith("v")) tag = tag.Substring("v".Length);

        return tag;
    }
}