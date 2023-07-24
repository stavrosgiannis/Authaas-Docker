using System.Security.Cryptography;
using System.Text.Json;

namespace Authaas_Docker.Models
{
    public class GitHubRelease
    {
        private static readonly HttpClient Client = new();
        public string? tag_name { get; set; }

        public string CalculateCurrentAppHash()
        {
            using (var stream = File.OpenRead(Application.ExecutablePath))
            {
                using var sha = SHA1.Create();
                var hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public string CalculateFileHash(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                using var sha = SHA1.Create();
                var hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public async Task DownloadLatestRelease(string repoOwner, string repoName, string tag)
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


        public async Task DownloadLatestReleaseIfUpdateAvailable(string currentHash, string repoOwner, string repoName)
        {
            var latestTag = await GetLatestReleaseTagAsync(repoOwner, repoName);
            if (latestTag == null) throw new Exception("Could not retrieve latest release tag.");

            if (!currentHash.Equals(latestTag, StringComparison.OrdinalIgnoreCase))
                await DownloadLatestRelease(repoOwner, repoName, latestTag);
        }


        public async Task<string?> GetLatestReleaseTagAsync(string repoOwner, string repoName)
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
