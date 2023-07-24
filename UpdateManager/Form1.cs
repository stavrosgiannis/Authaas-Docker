using System.Security.Cryptography;
using System.Text.Json;

namespace UpdateManager;

public partial class Form1 : Form
{
    private static readonly HttpClient Client = new();

    public Form1()
    {
        InitializeComponent();
    }


    /// <summary>
    ///     Returns a string with the current UTC date and time.
    /// </summary>
    /// <returns>
    ///     A string with the current UTC date and time in the format "[date] ".
    /// </returns>
    private static string DateForLog()
    {
        return $"[{DateTime.UtcNow}] ";
    }


    private void Form1_Load(object sender, EventArgs e)
    {
    }


    private async void Form1_Shown(object sender, EventArgs e)
    {
        //listBox1.Items.Add($"{DateForLog()}Checking for updates");
        //var updateManager = new UpdateManagerClass("https://example.com/latest-version.txt",
        //    "https://example.com/latest-version.exe");
        //await updateManager.CheckForUpdatesAsync();


        listBox1.Items.Add(DateForLog() + $"UpdateManager: {CalculateCurrentAppHash()}");
        if (File.Exists("AuthaasDocker.exe"))
            listBox1.Items.Add(DateForLog() + $"AuthaasDocker: {CalculateFileHash("AuthaasDocker.exe")}");
        else listBox1.Items.Add(DateForLog() + $"AuthaasDocker.exe not found!");


        var result = await GetLatestReleaseTagAsync("stavrosgiannis", "Authaas-Docker");
        listBox1.Items.Add(DateForLog() +
                           $"Newest AuthaasDocker: {result}");

        await DownloadLatestReleaseIfUpdateAvailable(CalculateFileHash("AuthaasDocker.exe"), "stavrosgiannis", "Authaas-Docker");
    }

    private string CalculateCurrentAppHash()
    {
        using (var stream = File.OpenRead(Application.ExecutablePath))
        {
            using var sha = SHA1.Create();
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    private string CalculateFileHash(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            using var sha = SHA1.Create();
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    private async Task DownloadLatestRelease(string repoOwner, string repoName, string tag)
    {
        var url = $"https://github.com/{repoOwner}/{repoName}/releases/download/{tag}/file";  // replace 'file' with the actual filename
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create("AuthaasDocker.exe");  // replace with the actual path where you want to save the file
        await stream.CopyToAsync(fileStream);
    }


    private async Task DownloadLatestReleaseIfUpdateAvailable(string currentHash, string repoOwner, string repoName)
    {
        var latestTag = await GetLatestReleaseTagAsync(repoOwner, repoName);
        if (latestTag == null)
        {
            throw new Exception("Could not retrieve latest release tag.");
        }

        if (!currentHash.Equals(latestTag, StringComparison.OrdinalIgnoreCase))
        {
            await DownloadLatestRelease(repoOwner, repoName, latestTag);
        }
    }



    private async Task<string?> GetLatestReleaseTagAsync(string repoOwner, string repoName)
    {
        var url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";
        Client.DefaultRequestHeaders.UserAgent
            .ParseAdd("Authaas-Docker-Updater"); // GitHub API requires a User-Agent header
        var json = await Client.GetStringAsync(url);
        var release = JsonSerializer.Deserialize<GitHubReleases>(json);

        // Remove the "commit-" prefix from the tag
        var tag = release?.tag_name;
        if (tag != null && tag.StartsWith("sha1-")) tag = tag.Substring("sha1-".Length);

        return tag;
    }
}