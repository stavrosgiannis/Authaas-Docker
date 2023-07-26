using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace Authaas_Docker.Models
{
    public class GitHubRelease
    {
        private static readonly HttpClient Client = new();
        public string? tag_name { get; set; }

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

        public static string CalculateFileHash(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                using var sha = SHA1.Create();
                var hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static async Task DownloadLatestRelease(string repoOwner, string repoName, string tag)
        {
            var url =
                $"https://github.com/{repoOwner}/{repoName}/releases/download/{tag}/file"; // replace 'file' with the actual filename
            var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var
                fileStream =
                    File.Create("AuthaasDocker.exe"); // replace with the actual path where you want to save the file
            await stream.CopyToAsync(fileStream);
        }


        public static async Task DownloadLatestReleaseIfUpdateAvailable(string currentVersion, string repoOwner, string repoName)
        {
            var latestTag = await GetLatestReleaseTagAsync(repoOwner, repoName);
            if (latestTag == null) throw new Exception("Could not retrieve latest release tag.");

            // Assuming that your tags are in the format "vX.Y.Z", we remove the leading 'v' 
            // to convert the tag into a version string
            var latestVersionStr = latestTag.TrimStart('v');
            var latestVersion = new Version(latestVersionStr);

            // Compare the current version with the latest version
            if (currentVersion.CompareTo(latestVersion) < 0)
                await DownloadLatestRelease(repoOwner, repoName, latestTag);
        }


        public static async Task<string?> GetLatestReleaseTagAsync(string repoOwner, string repoName)
        {
            var url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";
            Client.DefaultRequestHeaders.UserAgent
                .ParseAdd("Authaas-Docker-Updater"); // GitHub API requires a User-Agent header
            var json = await Client.GetStringAsync(url);
            var release = JsonSerializer.Deserialize<GitHubRelease>(json);

            // Remove the "commit-" prefix from the tag
            var tag = release?.tag_name;
            if (tag != null && tag.StartsWith("sha1-")) tag = tag.Substring("sha1-".Length);

            return tag;
        }
    }
}
