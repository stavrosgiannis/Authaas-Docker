using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace Authaas_Docker.Models
{
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

        /// <summary>
        ///     Downloads the latest release of a given repository from GitHub.
        /// </summary>
        /// <param name="repoOwner">The owner of the repository.</param>
        /// <param name="repoName">The name of the repository.</param>
        /// <param name="tag">The tag of the release.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
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


        /// <summary>
        ///     Downloads the latest release from a GitHub repository if the current version is older than the latest version.
        /// </summary>
        /// <param name="currentVersion">The current version of the application.</param>
        /// <param name="repoOwner">The owner of the GitHub repository.</param>
        /// <param name="repoName">The name of the GitHub repository.</param>
        /// <returns>
        ///     An asynchronous task that downloads the latest release from the GitHub repository if the current version is older
        ///     than the latest version.
        /// </returns>
        public static async Task DownloadLatestReleaseIfUpdateAvailable(Version? currentVersion, string repoOwner,
            string repoName)
        {
            var latestTag = await GetLatestReleaseTagAsync(repoOwner, repoName);
            if (latestTag == null) throw new Exception("Could not retrieve latest release tag.");

            // Assuming that your tags are in the format "vX.Y.Z.X", we remove the leading 'v' 
            // to convert the tag into a version string
            var latestVersionStr = latestTag.TrimStart('v');
            var latestVersion = new Version(latestVersionStr);

            // Compare the current version with the latest version
            if (currentVersion != null && currentVersion.CompareTo(latestVersion) < 0)
                await DownloadLatestRelease(repoOwner, repoName, latestTag);
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
            if (tag != null && tag.StartsWith("sha1-")) tag = tag.Substring("sha1-".Length);

            return tag;
        }
    }
}
